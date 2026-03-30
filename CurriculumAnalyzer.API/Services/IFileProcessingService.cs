namespace CurriculumAnalyzer.API.Services;

public interface IFileProcessingService
{
    Task<string> ExtractTextAsync(Stream fileStream, string fileType);
}
