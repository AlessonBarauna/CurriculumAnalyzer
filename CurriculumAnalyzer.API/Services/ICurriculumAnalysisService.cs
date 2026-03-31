using CurriculumAnalyzer.API.Models;

namespace CurriculumAnalyzer.API.Services;

public interface ICurriculumAnalysisService
{
    /// <summary>
    /// Orquestra o fluxo completo: extração de texto → cache → análise AI → persistência.
    /// Retorna o ID da análise (nova ou cacheada).
    /// </summary>
    Task<string> ProcessUploadAsync(
        Stream fileStream,
        string fileName,
        string fileExtension,
        long fileSize,
        UserContextModel userContext,
        string userId);
}
