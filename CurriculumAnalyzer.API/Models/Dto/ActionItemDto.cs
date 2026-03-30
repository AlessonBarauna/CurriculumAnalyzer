namespace CurriculumAnalyzer.API.Models.Dto;

public class ActionItemDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Priority { get; set; } = string.Empty;
    public string Timeline { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Checklist { get; set; } = new();
    public string EstimatedDuration { get; set; } = string.Empty;
}
