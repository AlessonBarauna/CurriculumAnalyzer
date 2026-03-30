using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CurriculumAnalyzer.API.Exceptions;
using CurriculumAnalyzer.API.Models;
using CurriculumAnalyzer.API.Models.Dto;

namespace CurriculumAnalyzer.API.Services;

public class GrokAiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GrokAiService> _logger;
    private const string GroqApiUrl = "https://api.groq.com/openai/v1/chat/completions";

    public GrokAiService(HttpClient httpClient, IConfiguration configuration, ILogger<GrokAiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AnalysisResponseDto> AnalyzeCurriculumAsync(string curriculumText, UserContextModel userContext)
    {
        var apiKey = _configuration["Groq:ApiKey"];
        if (string.IsNullOrEmpty(apiKey) || apiKey == "CONFIGURE_VIA_ENV")
            throw new GrokApiException("Groq API key não configurada. Defina Groq:ApiKey no appsettings.");

        var prompt = BuildAnalysisPrompt(curriculumText, userContext);

        var requestBody = new
        {
            model = "llama-3.3-70b-versatile",
            messages = new[] { new { role = "user", content = prompt } },
            temperature = 0.7,
            max_tokens = 4000
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, GroqApiUrl)
        {
            Content = JsonContent.Create(requestBody)
        };
        requestMessage.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var response = await _httpClient.SendAsync(requestMessage);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Grok API error {Status}: {Body}", response.StatusCode, responseContent);
            throw new GrokApiException($"Groq retornou erro {(int)response.StatusCode}. Tente novamente.");
        }

        var grokResponse = JsonSerializer.Deserialize<GrokApiResponse>(responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var content = grokResponse?.Choices?[0]?.Message?.Content
            ?? throw new GrokApiException("Resposta vazia recebida da API Groq.");

        return ParseGrokResponse(content);
    }

    private string BuildAnalysisPrompt(string curriculumText, UserContextModel userContext)
    {
        return $@"Você é um especialista em análise de currículos para a área de Tecnologia da Informação.

CURRÍCULO A ANALISAR:
---
{curriculumText}
---

CONTEXTO DO CANDIDATO:
- Nível desejado: {userContext.ExperienceLevel}
- Especialidade: {userContext.Specialization}
- Objetivo no mercado: {userContext.MarketObjective}
- Localização: {userContext.CurrentLocation}
{(userContext.TargetSalary.HasValue ? $"- Salário pretendido: R$ {userContext.TargetSalary}" : "")}

Faça uma análise profunda do currículo. Avalie estrutura, contato, resumo, experiência, habilidades, educação e projetos.

RETORNE APENAS UM JSON VÁLIDO (sem markdown, sem texto antes ou depois):
{{
  ""overallScore"": 75,
  ""scoreExplanation"": ""explicação do score aqui"",
  ""sections"": {{
    ""structure"": {{""score"": 80, ""feedback"": ""...""}},
    ""contact"": {{""score"": 90, ""feedback"": ""...""}},
    ""summary"": {{""score"": 70, ""feedback"": ""...""}},
    ""experience"": {{""score"": 75, ""feedback"": ""...""}},
    ""skills"": {{""score"": 80, ""feedback"": ""...""}},
    ""education"": {{""score"": 70, ""feedback"": ""...""}},
    ""projects"": {{""score"": 65, ""feedback"": ""...""}}
  }},
  ""strengths"": [
    {{""title"": ""..."", ""description"": ""..."", ""impact"": ""..."", ""priority"": ""high""}}
  ],
  ""weaknesses"": [
    {{""title"": ""..."", ""description"": ""..."", ""impact"": ""..."", ""priority"": ""critical"", ""example"": {{""bad"": ""..."", ""good"": ""...""}}, ""solutionSteps"": [""passo 1"", ""passo 2""]}}
  ],
  ""opportunities"": [
    {{""title"": ""..."", ""description"": ""..."", ""timelineWeeks"": 4, ""estimatedSalaryImpact"": 2000, ""difficulty"": ""medium""}}
  ],
  ""actionPlan"": [
    {{""timeline"": ""short-term"", ""priority"": ""urgent"", ""title"": ""..."", ""description"": ""..."", ""estimatedDuration"": ""2 semanas"", ""checklist"": [""item 1"", ""item 2""]}}
  ],
  ""jobRecommendations"": [
    {{""type"": ""startup"", ""fit"": ""strong"", ""strengths"": [""...""], ""improvements"": [""...""], ""preparationTips"": [""...""]}}
  ],
  ""estimatedSalaryRange"": {{""min"": 5000, ""max"": 8000, ""currency"": ""BRL""}}
}}";
    }

    private AnalysisResponseDto ParseGrokResponse(string responseText)
    {
        // Try to extract JSON if wrapped in markdown
        var jsonText = responseText.Trim();
        if (jsonText.Contains("```"))
        {
            var start = jsonText.IndexOf('{');
            var end = jsonText.LastIndexOf('}');
            if (start >= 0 && end > start)
                jsonText = jsonText[start..(end + 1)];
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        var result = JsonSerializer.Deserialize<AnalysisResponseDto>(jsonText, options)
            ?? throw new GrokApiException("Não foi possível interpretar a resposta da API Groq.");

        result.RawResponse = responseText;
        return result;
    }
}

public class GrokApiResponse
{
    public GrokChoice[]? Choices { get; set; }
}

public class GrokChoice
{
    public GrokMessage? Message { get; set; }
}

public class GrokMessage
{
    public string? Content { get; set; }
}
