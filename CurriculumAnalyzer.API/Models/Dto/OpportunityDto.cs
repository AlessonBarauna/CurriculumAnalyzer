namespace CurriculumAnalyzer.API.Models.Dto;

public class OpportunityDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TimelineWeeks { get; set; }
    public decimal EstimatedSalaryImpact { get; set; }
    public string Difficulty { get; set; } = string.Empty;
}
