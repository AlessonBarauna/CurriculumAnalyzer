using CurriculumAnalyzer.API.Models.Dto;

namespace CurriculumAnalyzer.API.Services;

public interface IAnalysisService
{
    Task<IEnumerable<object>> GetHistoryAsync(string userId);
    Task<AnalysisResponseDto?> GetByIdAsync(string id, string userId);
    Task<(AnalysisResponseDto Before, AnalysisResponseDto After)?> CompareAsync(string id1, string id2, string userId);
}
