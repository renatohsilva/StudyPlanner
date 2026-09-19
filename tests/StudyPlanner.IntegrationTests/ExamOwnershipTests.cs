using System.Net;
using System.Net.Http.Json;
using StudyPlanner.IntegrationTests.Helpers;

namespace StudyPlanner.IntegrationTests;

/// <summary>
/// Cobre o fix de autorização por propriedade (ExamOwnershipExtensions): um usuário autenticado
/// nunca deveria conseguir ler ou escrever no concurso de outro usuário só sabendo o Guid.
/// </summary>
[Collection("Integration")]
public class ExamOwnershipTests(StudyPlannerWebApplicationFactory factory)
{
    private record IdDto(Guid Id);

    private async Task<Guid> CreateExamAsync(HttpClient client, string name = "Exame")
    {
        var response = await client.PostAsJsonAsync("/api/exams", new
        {
            boardId = (Guid?)null,
            name,
            examDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6))
        });
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<IdDto>();
        return dto!.Id;
    }

    [Fact]
    public async Task Owner_CanReadOwnExam()
    {
        var (client, _) = await factory.CreateAuthenticatedClientAsync();
        var examId = await CreateExamAsync(client, "Exame do dono");

        var response = await client.GetAsync($"/api/exams/{examId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task OtherUser_CannotReadSomeoneElsesExam_ReturnsNotFound()
    {
        var (ownerClient, _) = await factory.CreateAuthenticatedClientAsync();
        var examId = await CreateExamAsync(ownerClient, "Exame privado do A");

        var (otherClient, _) = await factory.CreateAuthenticatedClientAsync();
        var response = await otherClient.GetAsync($"/api/exams/{examId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OtherUser_CannotCreateSubjectInSomeoneElsesExam()
    {
        var (ownerClient, _) = await factory.CreateAuthenticatedClientAsync();
        var examId = await CreateExamAsync(ownerClient, "Exame do A");

        var (otherClient, _) = await factory.CreateAuthenticatedClientAsync();
        var response = await otherClient.PostAsJsonAsync("/api/subjects", new
        {
            examId,
            name = "Materia invasora",
            weight = 0.5m
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OtherUser_CannotSetAvailabilityOnSomeoneElsesExam()
    {
        var (ownerClient, _) = await factory.CreateAuthenticatedClientAsync();
        var examId = await CreateExamAsync(ownerClient, "Exame do A");

        var (otherClient, _) = await factory.CreateAuthenticatedClientAsync();
        var response = await otherClient.PostAsJsonAsync($"/api/exams/{examId}/availability", new
        {
            slots = new[] { new { dayOfWeek = "Monday", hoursAvailable = 2m } }
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Owner_CanStillCreateSubjectInOwnExam_AfterOtherUserWasBlocked()
    {
        var (ownerClient, _) = await factory.CreateAuthenticatedClientAsync();
        var examId = await CreateExamAsync(ownerClient, "Exame do A 2");

        var response = await ownerClient.PostAsJsonAsync("/api/subjects", new
        {
            examId,
            name = "Materia legitima",
            weight = 0.5m
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
