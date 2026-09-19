using System.Net;
using System.Net.Http.Json;
using StudyPlanner.IntegrationTests.Helpers;

namespace StudyPlanner.IntegrationTests;

[Collection("Integration")]
public class AuthTests(StudyPlannerWebApplicationFactory factory)
{
    [Fact]
    public async Task Register_ReturnsTokenAndUserId()
    {
        var client = factory.CreateClient();
        var email = $"test-{Guid.NewGuid():N}@example.com";

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "Senha123!",
            name = "Novo Usuario"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(body);
        Assert.NotEmpty(body.Token);
        Assert.NotEqual(Guid.Empty, body.UserId);
        Assert.Equal(email, body.Email);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var client = factory.CreateClient();
        var email = $"test-{Guid.NewGuid():N}@example.com";
        var request = new { email, password = "Senha123!", name = "Usuario" };

        await client.PostAsJsonAsync("/api/auth/register", request);
        var secondAttempt = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Conflict, secondAttempt.StatusCode);
    }

    [Fact]
    public async Task Login_WithCorrectPassword_ReturnsToken()
    {
        var client = factory.CreateClient();
        var email = $"test-{Guid.NewGuid():N}@example.com";
        await client.PostAsJsonAsync("/api/auth/register", new { email, password = "Senha123!", name = "Usuario" });

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Senha123!" });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var body = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(body);
        Assert.NotEmpty(body.Token);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();
        var email = $"test-{Guid.NewGuid():N}@example.com";
        await client.PostAsJsonAsync("/api/auth/register", new { email, password = "Senha123!", name = "Usuario" });

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "SenhaErrada!" });

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = $"nao-existe-{Guid.NewGuid():N}@example.com", password = "Senha123!" });

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/exams/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithValidToken_IsReachable()
    {
        var (client, _) = await factory.CreateAuthenticatedClientAsync();

        // 404 (concurso inexistente) é uma resposta válida do handler — o importante aqui é que
        // NÃO seja 401: o token foi aceito e a requisição passou pela autenticação.
        var response = await client.GetAsync($"/api/exams/{Guid.NewGuid()}");

        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
