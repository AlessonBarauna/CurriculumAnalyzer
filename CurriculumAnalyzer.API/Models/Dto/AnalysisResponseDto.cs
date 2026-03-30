using System.Text.Json.Serialization;

namespace CurriculumAnalyzer.API.Models.Dto;

public class AnalysisResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string CurriculumId { get; set; } = string.Empty;
    public DateTime AnalysisDate { get; set; }

    [JsonPropertyName("overallScore")]
    public int OverallScore { get; set; }

    [JsonPropertyName("scoreExplanation")]
    public string ScoreExplanation { get; set; } = string.Empty;

    [JsonPropertyName("sections")]
    public Dictionary<string, SectionScoreDto> Sections { get; set; } = new();

    [JsonPropertyName("strengths")]
    public List<FeedbackDto> Strengths { get; set; } = new();

    [JsonPropertyName("weaknesses")]
    public List<FeedbackDto> Weaknesses { get; set; } = new();

    [JsonPropertyName("opportunities")]
    public List<OpportunityDto> Opportunities { get; set; } = new();

    [JsonPropertyName("actionPlan")]
    public List<ActionItemDto> ActionPlan { get; set; } = new();

    [JsonPropertyName("jobRecommendations")]
    public List<JobRecommendationDto> JobRecommendations { get; set; } = new();

    [JsonPropertyName("estimatedSalaryRange")]
    public SalaryRangeDto EstimatedSalaryRange { get; set; } = new();

    public string RawResponse { get; set; } = string.Empty;
}
