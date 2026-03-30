namespace CurriculumAnalyzer.API.Data.Entities;

public class CurriculumEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string FirebaseUrl { get; set; } = string.Empty;
    public string RawText { get; set; } = string.Empty;
    public string ExperienceLevel { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string MarketObjective { get; set; } = string.Empty;
    public decimal? TargetSalary { get; set; }
    public string CurrentLocation { get; set; } = string.Empty;
    public ICollection<AnalysisEntity> Analyses { get; set; } = new List<AnalysisEntity>();
}
