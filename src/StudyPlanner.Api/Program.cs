using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Application.Exams.Commands.CreateExam;
using StudyPlanner.Application.StudyPlans;
using StudyPlanner.Infrastructure.Auth;
using StudyPlanner.Infrastructure.Embeddings;
using StudyPlanner.Infrastructure.ExternalContent;
using StudyPlanner.Infrastructure.Llm;
using StudyPlanner.Infrastructure.Pdf;
using StudyPlanner.Infrastructure.Persistence;
using StudyPlanner.Infrastructure.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<StudyPlannerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("StudyPlannerDb"), o => o.UseVector()));
builder.Services.AddScoped<IStudyPlannerDbContext>(sp => sp.GetRequiredService<StudyPlannerDbContext>());

builder.Services.Configure<LocalFileStorageOptions>(builder.Configuration.GetSection("FileStorage"));
builder.Services.AddSingleton<IFileStorage, LocalFileStorage>();
builder.Services.AddSingleton<IPdfTextExtractor, PdfPigTextExtractor>();

builder.Services.Configure<OnnxEmbeddingGeneratorOptions>(builder.Configuration.GetSection("Embeddings"));
builder.Services.AddSingleton<IEmbeddingGenerator, OnnxEmbeddingGenerator>();

// Ingestão de material a partir de YouTube (legenda via yt-dlp — ver README) e links (texto da
// página). Sem custo, sem modelo novo: reaproveita o mesmo pipeline de chunking/embeddings do PDF.
builder.Services.Configure<YtDlpOptions>(builder.Configuration.GetSection("YtDlp"));
builder.Services.AddSingleton<IYouTubeTranscriptFetcher, YtDlpTranscriptFetcher>();
builder.Services.AddHttpClient<IWebPageTextExtractor, HtmlAgilityWebPageTextExtractor>();

// Llm:Provider = "Heuristic" (default, sem custo, regex determinístico) ou "Anthropic" (LLM real,
// requer Llm:Anthropic:ApiKey via user-secrets). Troca de provider sem alterar nenhum outro código,
// já que tudo depende apenas de ILlmClient.
builder.Services.Configure<AnthropicLlmOptions>(builder.Configuration.GetSection("Llm:Anthropic"));
var llmProvider = builder.Configuration.GetValue<string>("Llm:Provider") ?? "Heuristic";
if (string.Equals(llmProvider, "Anthropic", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddHttpClient<ILlmClient, AnthropicLlmClient>();
}
else
{
    builder.Services.AddSingleton<ILlmClient, HeuristicExamStructureExtractor>();
}

builder.Services.AddScoped<RankedTopicsProvider>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateExamCommand).Assembly));

// Auth: ASP.NET Identity (senha, hash, etc.) + JWT Bearer. A chave de assinatura vem de
// Jwt:Secret via user-secrets (mesmo padrão da chave da Anthropic) — nunca de appsettings.json.
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<StudyPlannerDbContext>();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<TokenService>();

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwtOptions.Secret) || jwtOptions.Secret.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Secret não configurado (ou curto demais — mínimo 32 caracteres). Como toda a API depende de " +
        "autenticação, um segredo fraco ou ausente compromete o sistema inteiro: configure via " +
        "'dotnet user-secrets set \"Jwt:Secret\" \"<valor aleatório de 32+ caracteres>\"' (ver README).");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret))
        };
    });

// Autenticado por padrão em todo endpoint — só quem tem [AllowAnonymous] (registro/login) escapa disso.
// Mais seguro que anotar [Authorize] manualmente em cada controller novo.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

const string DevCorsPolicy = "DevCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors(DevCorsPolicy);
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
