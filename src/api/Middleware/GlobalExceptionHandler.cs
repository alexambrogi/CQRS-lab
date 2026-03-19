using CQRS.POC.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CQRS.POC.Api.Middleware
{
    public class GlobalExceptionHandler: IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> _logger)
        {
            logger = _logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
        {
            logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

            var problemDetails = exception switch
            {
                ValidationException ve => new ProblemDetails
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Title = "Errore di validazione",
                    Detail = string.Join(", ", ve.Errors.Select(e => e.ErrorMessage))
                },
                NotFoundException => new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Risorsa non trovata",
                    Detail = exception.Message
                },
                DomainException => new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Errore di dominio",
                    Detail = exception.Message
                },
                _ => new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Errore interno del server",
                    Detail = "Si è verificato un errore imprevisto."
                }
            };

            context.Response.StatusCode = problemDetails.Status!.Value;
            await context.Response.WriteAsJsonAsync(problemDetails, ct);

            return true;
        }
    }
}