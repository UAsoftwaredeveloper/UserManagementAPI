using Microsoft.AspNetCore.Http;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    public LoggingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path;

        await _next(context);

        var statusCode = context.Response.StatusCode;
        Console.WriteLine($"[{DateTime.UtcNow}] {method} {path} responded {statusCode}");
    }
}