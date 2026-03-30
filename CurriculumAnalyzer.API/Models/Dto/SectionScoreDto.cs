namespace CurriculumAnalyzer.API.Models.Dto;

public class SectionScoreDto
{
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }
    public int Weight { get; set; }
    public string Feedback { get; set; } = string.Empty;
}
