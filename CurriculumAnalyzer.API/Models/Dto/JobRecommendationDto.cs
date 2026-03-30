namespace CurriculumAnalyzer.API.Models.Dto;

public class JobRecommendationDto
{
    public string Type { get; set; } = string.Empty;
    public string Fit { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Improvements { get; set; } = new();
    public List<string> PreparationTips { get; set; } = new();
}
