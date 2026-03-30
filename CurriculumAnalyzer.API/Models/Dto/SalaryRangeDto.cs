namespace CurriculumAnalyzer.API.Models.Dto;

public class SalaryRangeDto
{
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public string Currency { get; set; } = "BRL";
}
