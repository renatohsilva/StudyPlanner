using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Infrastructure.Persistence;
using StudyPlanner.IntegrationTests.Fakes;
using Testcontainers.PostgreSql;

namespace StudyPlanner.IntegrationTests;

/// <summary>
/// Sobe a API inteira em memória (WebApplicationFactory) contra um Postgres real e efêmero
/// (Testcontainers, imagem pgvector/pgvector:pg16 — mesma do docker-compose de dev) com as
/// migrações aplicadas. Serviços externos pesados/de rede (ONNX, PdfPig, yt-dlp, HTTP de páginas)
/// são trocados por fakes determinísticos — só o que depende de Postgres/Identity/MediatR/HTTP
/// roda de verdade. Compartilhado entre todos os testes da coleção "Integration" via
/// ICollectionFixture — um container só, não um por teste.
/// </summary>
public class StudyPlannerWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("pgvector/pgvector:pg16")
        .WithDatabase("studyplanner_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private readonly string _storageRoot = Path.Combine(Path.GetTempPath(), $"sp-test-storage-{Guid.NewGuid():N}");

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Program.cs lê ConnectionStrings/Jwt/etc direto de builder.Configuration ANTES de
        // builder.Build() (padrão do minimal hosting) — nesse ponto, overrides feitos via
        // ConfigureWebHost/ConfigureAppConfiguration ainda não existem (só passam a valer depois
        // de Build()), então esses valores "eager" nunca veem o override e ficam com o secret/
        // connection string reais de dev. Variáveis de ambiente, ao contrário, já fazem parte dos
        // provedores de configuração desde WebApplication.CreateBuilder(args), então setá-las AQUI,
        // antes de qualquer acesso a Services (que dispara a criação do host), garante que o mesmo
        // Jwt:Secret usado pra assinar o token (TokenService, via IOptions) seja o mesmo usado pra
        // validar (AddJwtBearer, lido eager) — e que a connection string aponte pro container efêmero,
        // não pro Postgres de desenvolvimento.
        Environment.SetEnvironmentVariable("ConnectionStrings__StudyPlannerDb", _postgres.GetConnectionString());
        Environment.SetEnvironmentVariable("Jwt__Secret", "integration-test-signing-secret-32chars-minimum");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "StudyPlannerIntegrationTests");
        Environment.SetEnvironmentVariable("Jwt__Audience", "StudyPlannerIntegrationTests");
        Environment.SetEnvironmentVariable("Llm__Provider", "Heuristic");
        Environment.SetEnvironmentVariable("FileStorage__RootPath", _storageRoot);

        // Força a criação do host (WebApplicationFactory é lazy) pra poder aplicar as migrações
        // antes de qualquer teste rodar.
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StudyPlannerDbContext>();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        if (Directory.Exists(_storageRoot)) Directory.Delete(_storageRoot, recursive: true);
        await _postgres.StopAsync();

        Environment.SetEnvironmentVariable("ConnectionStrings__StudyPlannerDb", null);
        Environment.SetEnvironmentVariable("Jwt__Secret", null);
        Environment.SetEnvironmentVariable("Jwt__Issuer", null);
        Environment.SetEnvironmentVariable("Jwt__Audience", null);
        Environment.SetEnvironmentVariable("Llm__Provider", null);
        Environment.SetEnvironmentVariable("FileStorage__RootPath", null);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IEmbeddingGenerator>();
            services.AddSingleton<IEmbeddingGenerator, FakeEmbeddingGenerator>();

            services.RemoveAll<IPdfTextExtractor>();
            services.AddSingleton<IPdfTextExtractor, FakePdfTextExtractor>();

            services.RemoveAll<IYouTubeTranscriptFetcher>();
            services.AddSingleton<IYouTubeTranscriptFetcher, FakeYouTubeTranscriptFetcher>();

            services.RemoveAll<IWebPageTextExtractor>();
            services.AddSingleton<IWebPageTextExtractor, FakeWebPageTextExtractor>();
        });
    }
}
