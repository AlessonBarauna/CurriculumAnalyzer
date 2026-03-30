namespace CurriculumAnalyzer.API.Data.Entities;

public class AnalysisEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CurriculumId { get; set; } = string.Empty;
    public CurriculumEntity Curriculum { get; set; } = null!;
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
    public int OverallScore { get; set; }
    public string ScoreExplanation { get; set; } = string.Empty;
    public string SectionsJson { get; set; } = string.Empty;
    public string StrengthsJson { get; set; } = string.Empty;
    public string WeaknessesJson { get; set; } = string.Empty;
    public string OpportunitiesJson { get; set; } = string.Empty;
    public string ActionPlanJson { get; set; } = string.Empty;
    public string JobRecommendationsJson { get; set; } = string.Empty;
    public decimal EstimatedMinSalary { get; set; }
    public decimal EstimatedMaxSalary { get; set; }
    public string RawGrokResponse { get; set; } = string.Empty;
}
