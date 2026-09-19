using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Application.Exams.Commands.CreateExam;
using StudyPlanner.Infrastructure.Embeddings;
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

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateExamCommand).Assembly));

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

app.UseAuthorization();

app.MapControllers();

app.Run();
