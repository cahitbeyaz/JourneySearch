using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ObiletJourneySearch.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "text/html";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // For AJAX/API requests, return JSON
            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                context.Request.Path.StartsWithSegments("/api"))
            {
                return HandleApiException(context, exception);
            }

            // For regular requests, redirect to error page
            context.Response.Redirect($"/Home/Error?code={context.Response.StatusCode}");
            return Task.CompletedTask;
        }

        private Task HandleApiException(HttpContext context, Exception exception)
        {
            var response = new
            {
                error = _environment.IsDevelopment() 
                    ? exception.Message 
                    : "An error occurred. Please try again later.",
                stackTrace = _environment.IsDevelopment() ? exception.StackTrace : null
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(jsonResponse);
        }
    }

    // Extension method to make middleware registration cleaner
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
