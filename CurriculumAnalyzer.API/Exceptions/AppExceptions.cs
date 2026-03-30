namespace CurriculumAnalyzer.API.Exceptions;

/// <summary>
/// Erro causado por entrada inválida do usuário — mapeia para 400 Bad Request.
/// </summary>
public class ValidationException(string message) : Exception(message);

/// <summary>
/// Recurso não encontrado no banco — mapeia para 404 Not Found.
/// </summary>
public class NotFoundException(string message) : Exception(message);

/// <summary>
/// Falha na comunicação com a API Groq — mapeia para 502 Bad Gateway.
/// </summary>
public class GrokApiException(string message) : Exception(message);
