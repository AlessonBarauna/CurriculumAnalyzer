using CurriculumAnalyzer.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

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
            ValidationException ex => (StatusCodes.Status400BadRequest, ex.Message),
            NotFoundException ex   => (StatusCodes.Status404NotFound,    ex.Message),
            GrokApiException ex    => (StatusCodes.Status502BadGateway,  ex.Message),
            _                      => (StatusCodes.Status500InternalServerError, "Erro interno no servidor.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception");

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new { error = message }, cancellationToken);
        return true;
    }
}
