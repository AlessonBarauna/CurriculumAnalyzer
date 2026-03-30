using CurriculumAnalyzer.API.Models;
using CurriculumAnalyzer.API.Models.Dto;

namespace CurriculumAnalyzer.API.Services;

public interface IGrokAiService
{
    Task<AnalysisResponseDto> AnalyzeCurriculumAsync(string curriculumText, UserContextModel userContext);
}
