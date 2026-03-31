using CurriculumAnalyzer.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace CurriculumAnalyzer.API.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, message) = exception switch
        {
            ValidationException ex  => (StatusCodes.Status400BadRequest,          ex.Message),
            NotFoundException ex    => (StatusCodes.Status404NotFound,             ex.Message),
            GrokApiException ex     => (StatusCodes.Status502BadGateway,           ex.Message),
            ArgumentException ex    => (StatusCodes.Status400BadRequest,           ex.Message),
            DbUpdateException       => (StatusCodes.Status401Unauthorized,         "Sessão expirada. Faça login novamente."),
            _                       => (StatusCodes.Status500InternalServerError,  "Erro interno no servidor.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new { error = message }, cancellationToken);
        return true;
    }
}
