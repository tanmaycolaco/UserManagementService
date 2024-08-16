namespace UserManagementService.Middelware;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Net;
using System.Threading.Tasks;


public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;

    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");

            var statusCode = (int)HttpStatusCode.InternalServerError;
            var message = "An error occurred while processing your request.";

            if (_env.IsDevelopment()) // Include stack trace in development for debugging
            {
                message += $" {ex.Message}\n{ex.StackTrace}";
            }

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(new { error = message }));
        }
    }
}