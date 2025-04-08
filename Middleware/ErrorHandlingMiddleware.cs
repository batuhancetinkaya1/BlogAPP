using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace BlogApp.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex, _environment.IsDevelopment());
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception, bool isDevelopment)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // AJAX requests return JSON response
            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var response = new
                {
                    success = false,
                    error = "An error occurred. Please try again later.",
                    details = isDevelopment ? exception.ToString() : null
                };

                var result = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(result);
            }
            else
            {
                // Store error details in TempData to be displayed on error page
                if (isDevelopment)
                {
                    context.Items["ErrorDetails"] = exception.ToString();
                }
                
                // Redirect to Error page
                context.Response.Redirect("/Home/Error");
            }
        }
    }

    // Extension method for configuring the middleware
    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
} 