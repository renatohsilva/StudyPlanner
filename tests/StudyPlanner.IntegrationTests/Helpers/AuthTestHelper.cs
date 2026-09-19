using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace StudyPlanner.IntegrationTests.Helpers;

public record AuthResponseDto(string Token, Guid UserId, string Name, string Email);

public static class AuthTestHelper
{
    /// <summary>Registra um usuário novo (email aleatório — evita colisão entre testes que compartilham o mesmo Postgres) e devolve um HttpClient já autenticado.</summary>
    public static async Task<(HttpClient Client, Guid UserId)> CreateAuthenticatedClientAsync(
        this StudyPlannerWebApplicationFactory factory, string? password = null)
    {
        var client = factory.CreateClient();
        var email = $"test-{Guid.NewGuid():N}@example.com";

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = password ?? "Senha123!",
            name = "Test User"
        });
        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>()
            ?? throw new InvalidOperationException("Registro não retornou um token.");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        return (client, auth.UserId);
    }
}
