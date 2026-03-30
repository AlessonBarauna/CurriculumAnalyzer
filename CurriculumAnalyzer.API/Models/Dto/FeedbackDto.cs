namespace CurriculumAnalyzer.API.Models.Dto;

public class FeedbackDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Priority { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public ExampleDto? Example { get; set; }
    public List<string> SolutionSteps { get; set; } = new();
}

public class ExampleDto
{
    public string Bad { get; set; } = string.Empty;
    public string Good { get; set; } = string.Empty;
}
