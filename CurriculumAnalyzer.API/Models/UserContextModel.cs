namespace CurriculumAnalyzer.API.Models;

public class UserContextModel
{
    public string ExperienceLevel { get; set; } = "mid-level";
    public string Specialization { get; set; } = "backend";
    public string MarketObjective { get; set; } = string.Empty;
    public decimal? TargetSalary { get; set; }
    public string CurrentLocation { get; set; } = "Brazil";
}