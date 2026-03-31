namespace CurriculumAnalyzer.API.Data.Entities;

public class UserEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<CurriculumEntity> Curriculums { get; set; } = new List<CurriculumEntity>();
}
