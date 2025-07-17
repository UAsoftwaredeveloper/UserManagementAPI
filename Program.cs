using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UserManagementAPI.Models;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// In-memory user list
var users = new List<User>();

// Helper method for validation
bool IsValidUser(User user)
{
    if (string.IsNullOrWhiteSpace(user.FirstName) ||
        string.IsNullOrWhiteSpace(user.LastName) ||
        string.IsNullOrWhiteSpace(user.Email) ||
        string.IsNullOrWhiteSpace(user.Department))
        return false;

    // Simple email validation
    var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
    if (!Regex.IsMatch(user.Email, emailPattern))
        return false;

    return true;
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 1. Error-handling middleware (first)
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var errorJson = System.Text.Json.JsonSerializer.Serialize(new { error = "Internal server error." });
        await context.Response.WriteAsync(errorJson);
        Console.WriteLine($"[{DateTime.UtcNow}] Unhandled exception: {ex.Message}");
    }
});

// 2. Authentication middleware (next)
app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
    {
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";
        var errorJson = System.Text.Json.JsonSerializer.Serialize(new { error = "Unauthorized: Missing or invalid token." });
        await context.Response.WriteAsync(errorJson);
        return;
    }

    var token = authHeader.Substring("Bearer ".Length).Trim();

    // Simple token validation (replace with real validation logic as needed)
    if (token != "your_valid_token")
    {
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";
        var errorJson = System.Text.Json.JsonSerializer.Serialize(new { error = "Unauthorized: Invalid token." });
        await context.Response.WriteAsync(errorJson);
        return;
    }

    await next();
});

// 3. Logging middleware (last)
app.Use(async (context, next) =>
{
    var method = context.Request.Method;
    var path = context.Request.Path;

    await next();

    var statusCode = context.Response.StatusCode;
    Console.WriteLine($"[{DateTime.UtcNow}] {method} {path} responded {statusCode}");
});

// Get all users (optimized with AsReadOnly)
app.MapGet("/users", () =>
{
    try
    {
        return Results.Ok(users.AsReadOnly());
    }
    catch (Exception ex)
    {
        return Results.Problem($"Unexpected error: {ex.Message}");
    }
});

// Get user by Id
app.MapGet("/users/{id:int}", (int id) =>
{
    try
    {
        var user = users.FirstOrDefault(u => u.Id == id);
        return user is not null ? Results.Ok(user) : Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.Problem($"Unexpected error: {ex.Message}");
    }
});

// Create user
app.MapPost("/users", (User user) =>
{
    try
    {
        if (!IsValidUser(user))
            return Results.BadRequest("Invalid user data.");

        user.Id = users.Count > 0 ? users.Max(u => u.Id) + 1 : 1;
        users.Add(user);
        return Results.Created($"/users/{user.Id}", user);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Unexpected error: {ex.Message}");
    }
});

// Update user
app.MapPut("/users/{id:int}", (int id, User updatedUser) =>
{
    try
    {
        if (!IsValidUser(updatedUser))
            return Results.BadRequest("Invalid user data.");

        var user = users.FirstOrDefault(u => u.Id == id);
        if (user is null) return Results.NotFound();

        user.FirstName = updatedUser.FirstName;
        user.LastName = updatedUser.LastName;
        user.Email = updatedUser.Email;
        user.Department = updatedUser.Department;

        return Results.Ok(user);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Unexpected error: {ex.Message}");
    }
});

// Delete user
app.MapDelete("/users/{id:int}", (int id) =>
{
    try
    {
        var user = users.FirstOrDefault(u => u.Id == id);
        if (user is null) return Results.NotFound();

        users.Remove(user);
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.Problem($"Unexpected error: {ex.Message}");
    }
});

app.Run();