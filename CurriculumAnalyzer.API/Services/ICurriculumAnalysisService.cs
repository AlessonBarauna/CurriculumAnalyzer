using CurriculumAnalyzer.API.Models;
using CurriculumAnalyzer.API.Models.Dto;

namespace CurriculumAnalyzer.API.Services;

public interface ICurriculumAnalysisService
{
    Task<AnalysisResponseDto> AnalyzeCurriculumAsync(string curriculumId, string curriculumText, UserContextModel userContext);
}
