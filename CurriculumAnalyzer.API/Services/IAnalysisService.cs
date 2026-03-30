using CurriculumAnalyzer.API.Models.Dto;

namespace CurriculumAnalyzer.API.Services;

public interface IAnalysisService
{
    Task<IEnumerable<object>> GetHistoryAsync();
    Task<AnalysisResponseDto?> GetByIdAsync(string id);
    Task<(AnalysisResponseDto Before, AnalysisResponseDto After)?> CompareAsync(string id1, string id2);
}
