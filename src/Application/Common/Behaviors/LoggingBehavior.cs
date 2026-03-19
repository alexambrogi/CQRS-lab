using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CQRS.POC.Application.Common.Behaviors
{

    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        //(ILogger<LoggingBehavior<TRequest, TResponse>> logger) 
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> _logger)
        {
            this.logger = _logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            var requestName = typeof(TRequest).Name;
            var stopwatch = Stopwatch.StartNew();

            logger.LogInformation("▶ Handling {RequestName}", requestName);

            try
            {
                var response = await next();
                stopwatch.Stop();

                logger.LogInformation(
                    "✔ Completed {RequestName} in {Elapsed}ms",
                    requestName, stopwatch.ElapsedMilliseconds);

                if (stopwatch.ElapsedMilliseconds > 500)
                    logger.LogWarning(
                        "⚠ SLOW REQUEST: {RequestName} took {Elapsed}ms",
                        requestName, stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.LogError(ex,
                    "✖ Error in {RequestName} after {Elapsed}ms",
                    requestName, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
