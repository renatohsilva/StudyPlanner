using System.Net.Http.Json;
using StudyPlanner.IntegrationTests.Helpers;

namespace StudyPlanner.IntegrationTests;

/// <summary>
/// Reproduz automatizado o teste manual feito por curl durante o desenvolvimento: dois tópicos de
/// peso igual empatam no ranking até um deles ser respondido corretamente várias vezes — aí ele cai
/// no ranking (mastery alto reduz prioridade) e o tópico nunca estudado assume o topo.
/// </summary>
[Collection("Integration")]
public class PriorityEngineFlowTests(StudyPlannerWebApplicationFactory factory)
{
    private record IdDto(Guid Id);
    private record TodayPlanItemDto(Guid TopicId, string TopicName, decimal PriorityScore, decimal Mastery, int AttemptsCount, string SuggestedType);
    private record TodayPlanDto(Guid ExamId, DateOnly Today, List<TodayPlanItemDto> Items);
    private record AttemptResultDto(Guid AttemptId, bool IsCorrect, string CorrectAnswer);

    [Fact]
    public async Task RepeatedCorrectAnswers_LowerTopicPriority_AndPromoteUntouchedTopic()
    {
        var (client, _) = await factory.CreateAuthenticatedClientAsync();

        var examId = await PostForIdAsync(client, "/api/exams", new
        {
            boardId = (Guid?)null,
            name = "Concurso teste priority engine",
            examDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6))
        });

        var subject1 = await PostForIdAsync(client, "/api/subjects", new { examId, name = "Direito Constitucional", weight = 0.5m });
        var topic1 = await PostForIdAsync(client, "/api/topics", new { subjectId = subject1, parentTopicId = (Guid?)null, name = "Direitos Fundamentais", estimatedIncidence = 1.0m });

        var subject2 = await PostForIdAsync(client, "/api/subjects", new { examId, name = "Administracao Publica", weight = 0.5m });
        var topic2 = await PostForIdAsync(client, "/api/topics", new { subjectId = subject2, parentTopicId = (Guid?)null, name = "Licitacoes", estimatedIncidence = 1.0m });

        var planBefore = await GetTodayPlanAsync(client, examId);
        var itemsBefore = planBefore.Items.ToDictionary(i => i.TopicId);
        Assert.Equal(itemsBefore[topic1].PriorityScore, itemsBefore[topic2].PriorityScore);

        var questionId = await PostForIdAsync(client, "/api/questions", new
        {
            statement = "Pergunta de teste",
            alternativesJson = (string?)null,
            correctAnswer = "A",
            source = "UserCreated",
            boardId = (Guid?)null,
            year = (int?)null,
            topicIds = new[] { topic1 }
        });

        for (var i = 0; i < 5; i++)
        {
            var attemptResponse = await client.PostAsJsonAsync($"/api/questions/{questionId}/attempt", new
            {
                chosenAnswer = "A",
                timeSpentSeconds = (int?)null
            });
            attemptResponse.EnsureSuccessStatusCode();
            var result = await attemptResponse.Content.ReadFromJsonAsync<AttemptResultDto>();
            Assert.True(result!.IsCorrect);
        }

        var planAfter = await GetTodayPlanAsync(client, examId);
        var itemsAfter = planAfter.Items.ToDictionary(i => i.TopicId);

        Assert.True(itemsAfter[topic1].Mastery > 0m);
        Assert.Equal(5, itemsAfter[topic1].AttemptsCount);
        Assert.True(
            itemsAfter[topic2].PriorityScore > itemsAfter[topic1].PriorityScore,
            "Tópico nunca estudado deveria ultrapassar em prioridade o tópico já dominado.");
        Assert.Equal("Review", itemsAfter[topic1].SuggestedType);
        Assert.Equal("NewContent", itemsAfter[topic2].SuggestedType);
    }

    [Fact]
    public async Task StudySession_IncreasesCoverage_ButNotMastery()
    {
        var (client, _) = await factory.CreateAuthenticatedClientAsync();

        var examId = await PostForIdAsync(client, "/api/exams", new
        {
            boardId = (Guid?)null,
            name = "Concurso teste coverage",
            examDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6))
        });
        var subjectId = await PostForIdAsync(client, "/api/subjects", new { examId, name = "Materia", weight = 1.0m });
        var topicId = await PostForIdAsync(client, "/api/topics", new { subjectId, parentTopicId = (Guid?)null, name = "Topico", estimatedIncidence = 1.0m });

        var metricsBefore = await client.GetFromJsonAsync<MetricsDto>($"/api/exams/{examId}/metrics");
        Assert.Equal(0m, metricsBefore!.Coverage);

        var sessionResponse = await client.PostAsJsonAsync("/api/study-sessions", new
        {
            topicId,
            studyPlanItemId = (Guid?)null,
            durationMinutes = 30
        });
        sessionResponse.EnsureSuccessStatusCode();

        var metricsAfter = await client.GetFromJsonAsync<MetricsDto>($"/api/exams/{examId}/metrics");

        Assert.Equal(1m, metricsAfter!.Coverage);
        Assert.Equal(0, metricsAfter.TopicsWithAttempts);
        Assert.Equal(0.5m, metricsAfter.StudyHoursTotal);
    }

    private record MetricsDto(decimal Coverage, int TopicsWithAttempts, decimal StudyHoursTotal);

    private static async Task<Guid> PostForIdAsync(HttpClient client, string url, object body)
    {
        var response = await client.PostAsJsonAsync(url, body);
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<IdDto>();
        return dto!.Id;
    }

    private static async Task<TodayPlanDto> GetTodayPlanAsync(HttpClient client, Guid examId)
    {
        var plan = await client.GetFromJsonAsync<TodayPlanDto>($"/api/study-plan/today?examId={examId}&top=10");
        return plan!;
    }
}
