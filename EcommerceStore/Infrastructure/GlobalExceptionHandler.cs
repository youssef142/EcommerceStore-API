using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceStore.Infrastructure
{
    /// <summary>
    /// Replaces the try/catch + TempData["Error"] blocks that used to live in every MVC action.
    /// Controllers just throw; this turns it into an RFC 7807 problem response.
    /// </summary>
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            int status;
            string title;
            string? detail;

            if (exception is BadHttpRequestException badRequest)
            {
                status = badRequest.StatusCode;
                title = "Bad request.";
                detail = badRequest.Message;
                _logger.LogWarning(exception, "Bad request: {Message}", badRequest.Message);
            }
            else
            {
                status = StatusCodes.Status500InternalServerError;
                title = "An unexpected error occurred.";
                detail = _env.IsDevelopment() ? exception.ToString() : null;
                _logger.LogError(exception, "Unhandled exception for {Method} {Path}",
                    httpContext.Request.Method, httpContext.Request.Path);
            }

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            };
            problem.Extensions["traceId"] = httpContext.TraceIdentifier;

            httpContext.Response.StatusCode = status;
            await httpContext.Response.WriteAsJsonAsync(
                problem, (System.Text.Json.JsonSerializerOptions?)null, "application/problem+json", cancellationToken);
            return true;
        }
    }
}
