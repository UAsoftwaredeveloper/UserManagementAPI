using Microsoft.AspNetCore.Http;
using System.Text.Json;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    public ErrorHandlingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            var errorJson = JsonSerializer.Serialize(new { error = "Internal server error." });
            await context.Response.WriteAsync(errorJson);
            Console.WriteLine($"[{DateTime.UtcNow}] Unhandled exception: {ex.Message}");
        }
    }
}