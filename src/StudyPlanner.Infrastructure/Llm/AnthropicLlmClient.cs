using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Application.Notices.Models;

namespace StudyPlanner.Infrastructure.Llm;

/// <summary>
/// Único adapter que fala com um provider de LLM externo. Usa tool-use da Anthropic Messages API
/// para forçar saída estruturada (JSON schema), em vez de parsear texto livre — reduz o risco de
/// resposta malformada bloquear o pipeline determinístico do resto do sistema.
/// </summary>
public class AnthropicLlmClient(HttpClient httpClient, IOptions<AnthropicLlmOptions> options) : ILlmClient
{
    private readonly AnthropicLlmOptions _options = options.Value;

    private const string ToolName = "extract_exam_structure";

    private const string SystemPrompt =
        "Você é um classificador de editais de concurso público brasileiro. Receberá o trecho de " +
        "\"conteúdo programático\" de um edital, possivelmente com formatação ruidosa. Extraia as " +
        "disciplinas e, para cada uma, os tópicos/subtópicos listados, preservando a ordem e o texto " +
        "original dos nomes. Se o edital declarar pesos ou quantidade de questões por disciplina, use-os " +
        "para estimar 'weight' (0 a 1, somando ~1 entre as disciplinas); caso contrário, distribua os pesos " +
        "igualmente. Estime 'estimatedIncidence' de cada tópico dentro da disciplina (0 a 1, somando ~1 " +
        "entre os tópicos da mesma disciplina) com base na posição/ênfase no texto, quando não houver dado explícito.";

    public async Task<ExtractedExamStructure> ExtractExamStructureAsync(string programContentText, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "LLM API key não configurada (Llm:Anthropic:ApiKey). Configure para habilitar a extração automática do edital.");
        }

        var requestBody = new JsonObject
        {
            ["model"] = _options.Model,
            ["max_tokens"] = _options.MaxTokens,
            ["system"] = SystemPrompt,
            ["messages"] = new JsonArray
            {
                new JsonObject
                {
                    ["role"] = "user",
                    ["content"] = programContentText
                }
            },
            ["tools"] = new JsonArray { BuildToolSchema() },
            ["tool_choice"] = new JsonObject { ["type"] = "tool", ["name"] = ToolName }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.BaseUrl)
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Add("x-api-key", _options.ApiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Falha ao chamar o LLM ({(int)response.StatusCode}): {responseBody}");
        }

        return ParseToolUseResponse(responseBody);
    }

    private static JsonObject BuildToolSchema() => new()
    {
        ["name"] = ToolName,
        ["description"] = "Estrutura de disciplinas e tópicos extraída do conteúdo programático do edital.",
        ["input_schema"] = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["subjects"] = new JsonObject
                {
                    ["type"] = "array",
                    ["items"] = new JsonObject
                    {
                        ["type"] = "object",
                        ["properties"] = new JsonObject
                        {
                            ["name"] = new JsonObject { ["type"] = "string" },
                            ["weight"] = new JsonObject { ["type"] = "number" },
                            ["topics"] = new JsonObject
                            {
                                ["type"] = "array",
                                ["items"] = new JsonObject
                                {
                                    ["type"] = "object",
                                    ["properties"] = new JsonObject
                                    {
                                        ["name"] = new JsonObject { ["type"] = "string" },
                                        ["estimatedIncidence"] = new JsonObject { ["type"] = "number" }
                                    },
                                    ["required"] = new JsonArray { "name", "estimatedIncidence" }
                                }
                            }
                        },
                        ["required"] = new JsonArray { "name", "weight", "topics" }
                    }
                }
            },
            ["required"] = new JsonArray { "subjects" }
        }
    };

    private static ExtractedExamStructure ParseToolUseResponse(string responseBody)
    {
        using var doc = JsonDocument.Parse(responseBody);
        var contentBlocks = doc.RootElement.GetProperty("content");

        foreach (var block in contentBlocks.EnumerateArray())
        {
            if (block.GetProperty("type").GetString() != "tool_use") continue;

            var input = block.GetProperty("input");
            var structure = input.Deserialize<ExtractedExamStructure>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (structure is not null) return structure;
        }

        throw new InvalidOperationException("Resposta do LLM não continha um bloco tool_use válido.");
    }
}
