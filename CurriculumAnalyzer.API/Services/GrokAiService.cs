using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CurriculumAnalyzer.API.Exceptions;
using CurriculumAnalyzer.API.Models;
using CurriculumAnalyzer.API.Models.Dto;

namespace CurriculumAnalyzer.API.Services;

public class GrokAiService : IGrokAiService
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

    private const string SystemMessage =
        "Você é um especialista sênior em recrutamento tech no mercado brasileiro, com 15 anos de experiência " +
        "avaliando currículos para empresas como Nubank, iFood, Mercado Livre, TOTVS e startups de alto crescimento. " +
        "Sua análise é direta, honesta e específica — você nunca dá feedback genérico nem elogios vazios. " +
        "Você sabe que um feedback que não aponta problemas reais não ajuda o candidato a crescer.";

    public async Task<AnalysisResponseDto> AnalyzeCurriculumAsync(string curriculumText, UserContextModel userContext)
    {
        var apiKey = _configuration["Groq:ApiKey"];
        if (string.IsNullOrEmpty(apiKey) || apiKey == "CONFIGURE_VIA_ENV")
            throw new GrokApiException("Groq API key não configurada. Defina Groq:ApiKey no appsettings.");

        var prompt = BuildAnalysisPrompt(curriculumText, userContext);
        return await CallWithFallbackAsync(prompt, apiKey);
    }

    private string BuildAnalysisPrompt(string curriculumText, UserContextModel userContext)
    {
        return $@"CURRÍCULO A ANALISAR:
---
{curriculumText}
---

PERFIL DO CANDIDATO:
- Nível: {userContext.ExperienceLevel}
- Especialidade: {userContext.Specialization}
- Objetivo: {userContext.MarketObjective}
- Localização: {userContext.CurrentLocation}
{(userContext.TargetSalary.HasValue ? $"- Pretensão salarial: R$ {userContext.TargetSalary}" : "")}

CALIBRAÇÃO DE SCORES (aplique com rigor):
- 90-100: Excepcional, raridade no mercado
- 75-89: Forte, acima da média dos candidatos
- 60-74: Adequado, cumpre requisitos básicos
- 40-59: Abaixo do esperado para o nível
- 0-39: Crítico, compromete a candidatura

CONTEXTO DO MERCADO BRASILEIRO DE TI (2025-2026):
- Tecnologias muito demandadas: TypeScript, React, Node.js, Python, Go, Kotlin, .NET, AWS, Docker, Kubernetes, SQL
- Salários CLT — Junior: R$3k-6k | Mid: R$7k-12k | Senior: R$12k-22k
- Salários PJ — Junior: R$4.5k-8k | Mid: R$10k-16k | Senior: R$18k-35k | Especialista: R$25k-45k+

REGRAS OBRIGATÓRIAS:
1. Cite trechos REAIS do currículo ao justificar fraquezas — nunca generalize
2. Em ""example.bad"" use texto real do currículo; em ""example.good"" reescreva com melhoria concreta
3. O overallScore deve ser a média ponderada das seções (experiência tem peso 2x)
4. solutionSteps devem ser ações concretas e ordenadas, não conselhos genéricos
5. estimatedSalaryRange deve refletir o nível e localização informados, não inventar valores

RETORNE APENAS JSON VÁLIDO (sem markdown, sem texto fora do JSON):
{{
  ""overallScore"": <número 0-100>,
  ""scoreExplanation"": ""<2-3 frases explicando o score, mencionando pontos fortes e críticos>"",
  ""sections"": {{
    ""structure"": {{""score"": <0-100>, ""feedback"": ""<feedback específico sobre layout, organização, extensão>""}},
    ""contact"": {{""score"": <0-100>, ""feedback"": ""<LinkedIn, GitHub, email, telefone — o que falta ou está errado>""}},
    ""summary"": {{""score"": <0-100>, ""feedback"": ""<avalie se existe, se é objetivo, se comunica proposta de valor>""}},
    ""experience"": {{""score"": <0-100>, ""feedback"": ""<avalie resultados mensuráveis, verbos de ação, relevância para o nível>""}},
    ""skills"": {{""score"": <0-100>, ""feedback"": ""<avalie organização, relevância das tecnologias para o objetivo>""}},
    ""education"": {{""score"": <0-100>, ""feedback"": ""<graduação, certificações, cursos — adequação ao nível pretendido>""}},
    ""projects"": {{""score"": <0-100>, ""feedback"": ""<projetos pessoais, links, impacto — ou ausência deles>""}}
  }},
  ""strengths"": [
    {{""title"": ""..."", ""description"": ""..."", ""impact"": ""..."", ""priority"": ""high""}}
  ],
  ""weaknesses"": [
    {{""title"": ""..."", ""description"": ""..."", ""impact"": ""..."", ""priority"": ""critical"", ""example"": {{""bad"": ""<trecho real>"", ""good"": ""<versão melhorada>""}}, ""solutionSteps"": [""passo concreto 1"", ""passo concreto 2""]}}
  ],
  ""opportunities"": [
    {{""title"": ""..."", ""description"": ""..."", ""timelineWeeks"": <número>, ""estimatedSalaryImpact"": <número em BRL>, ""difficulty"": ""low|medium|high""}}
  ],
  ""actionPlan"": [
    {{""timeline"": ""short-term|mid-term|long-term"", ""priority"": ""urgent|high|medium"", ""title"": ""..."", ""description"": ""..."", ""estimatedDuration"": ""..."", ""checklist"": [""item 1"", ""item 2""]}}
  ],
  ""jobRecommendations"": [
    {{""type"": ""startup|enterprise|consultoria|remoto-internacional"", ""fit"": ""strong|moderate|weak"", ""strengths"": [""...""], ""improvements"": [""...""], ""preparationTips"": [""...""]}}
  ],
  ""estimatedSalaryRange"": {{""min"": <número>, ""max"": <número>, ""currency"": ""BRL""}}
}}";
    }

    private async Task<AnalysisResponseDto> CallWithFallbackAsync(string prompt, string apiKey)
    {
        const string primaryModel = "llama-3.3-70b-versatile";
        const string fallbackModel = "llama-3.1-8b-instant";

        try
        {
            return await CallWithRetryAsync(primaryModel, prompt, apiKey);
        }
        catch (GrokApiException ex)
        {
            _logger.LogWarning("Modelo primário {Model} falhou: {Message}. Tentando fallback {Fallback}...",
                primaryModel, ex.Message, fallbackModel);

            return await CallWithRetryAsync(fallbackModel, prompt, apiKey);
        }
    }

    private async Task<AnalysisResponseDto> CallWithRetryAsync(string model, string prompt, string apiKey, int maxRetries = 2)
    {
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                var content = await SendRequestAsync(model, prompt, apiKey);
                return ParseGrokResponse(content);
            }
            catch (GrokApiException ex)
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt + 1)); // 2s, 4s
                _logger.LogWarning("Tentativa {Attempt}/{Max} falhou para {Model}: {Message}. Aguardando {Delay}s...",
                    attempt + 1, maxRetries, model, ex.Message, delay.TotalSeconds);
                await Task.Delay(delay);
            }
        }

        // última tentativa — deixa a exceção propagar
        var lastContent = await SendRequestAsync(model, prompt, apiKey);
        return ParseGrokResponse(lastContent);
    }

    private async Task<string> SendRequestAsync(string model, string prompt, string apiKey)
    {
        var requestBody = new
        {
            model,
            messages = new[]
            {
                new { role = "system", content = SystemMessage },
                new { role = "user",   content = prompt }
            },
            temperature = 0.3,
            max_tokens = 6000
        };

        var request = new HttpRequestMessage(HttpMethod.Post, GroqApiUrl)
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var response = await _httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Groq API erro {Status} [{Model}]: {Body}", response.StatusCode, model, responseContent);
            throw new GrokApiException($"Groq retornou erro {(int)response.StatusCode}. Tente novamente.");
        }

        var grokResponse = JsonSerializer.Deserialize<GrokApiResponse>(responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return grokResponse?.Choices?[0]?.Message?.Content
            ?? throw new GrokApiException("Resposta vazia recebida da API Groq.");
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
