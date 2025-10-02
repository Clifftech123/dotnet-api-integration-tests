using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TestWebApi.Shared.TestWebMiddleWare;

namespace TestWebApi.Shared.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (status, title, code, errors, level) = Map(exception);

            if (!httpContext.Response.HasStarted)
            {
                Log(level, exception, code);

                var problem = new ProblemDetails
                {
                    Status = status,
                    Title = title,
                    Detail = exception.Message,
                    Instance = httpContext.Request.Path
                };
                problem.Extensions["traceId"] = httpContext.TraceIdentifier;
                problem.Extensions["code"] = code;
                if (errors is not null)
                    problem.Extensions["errors"] = errors;

                if (_env.IsDevelopment())
                {
                    problem.Extensions["exception"] = exception.GetType().Name;
                    problem.Extensions["stackTrace"] = exception.StackTrace;
                }

                httpContext.Response.StatusCode = status;
                httpContext.Response.ContentType = "application/problem+json";
                await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            }

            return true;
        }

        private static (int status, string title, string code, string[]? errors, LogLevel level) Map(Exception ex) =>
            ex switch
            {
                ValidationException ve => (StatusCodes.Status400BadRequest, "Validation Failed", ve.Code, ve.Errors.Select(e => $"{e.Field}: {e.Message}").ToArray(), LogLevel.Information),
                ProductNotFoundException or CategoryNotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found", ((ProjectException)ex).Code, null, LogLevel.Information),
                DuplicateCategoryNameException or DuplicateProductNameException => (StatusCodes.Status409Conflict, "Conflict", ((ProjectException)ex).Code, null, LogLevel.Information),
                CategoryDeleteNotAllowedException cd => (StatusCodes.Status409Conflict, "Delete Not Allowed", cd.Code, new[] { $"Category {cd.CategoryId} still has {cd.ProductCount} product(s)" }, LogLevel.Information),
                PaginationOutOfRangeException p => (StatusCodes.Status400BadRequest, "Pagination Out Of Range", p.Code, new[] { $"Requested={p.RequestedPage}; Total={p.TotalPages}" }, LogLevel.Information),
                RepositoryOperationException => (StatusCodes.Status500InternalServerError, "Repository Failure", ((ProjectException)ex).Code, null, LogLevel.Error),
                ProjectException pe => (StatusCodes.Status400BadRequest, "Request Error", pe.Code, null, LogLevel.Warning),
                _ => (StatusCodes.Status500InternalServerError, "Unexpected Error", "UNHANDLED", null, LogLevel.Error)
            };

        private void Log(LogLevel level, Exception ex, string code)
        {
            switch (level)
            {
                case LogLevel.Information: _logger.LogInformation(ex, "[{Code}] {Message}", code, ex.Message); break;
                case LogLevel.Warning: _logger.LogWarning(ex, "[{Code}] {Message}", code, ex.Message); break;
                default: _logger.LogError(ex, "[{Code}] {Message}", code, ex.Message); break;
            }
        }
    }
}