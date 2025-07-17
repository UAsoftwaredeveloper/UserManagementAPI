using Microsoft.AspNetCore.Http;
using System.Text.Json;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    public AuthenticationMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var errorJson = JsonSerializer.Serialize(new { error = "Unauthorized: Missing or invalid token." });
            await context.Response.WriteAsync(errorJson);
            return;
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();
        if (token != "your_valid_token")
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var errorJson = JsonSerializer.Serialize(new { error = "Unauthorized: Invalid token." });
            await context.Response.WriteAsync(errorJson);
            return;
        }

        await _next(context);
    }
}