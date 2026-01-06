using Hotel.Application;
using Hotel.Infrastructure;
using Hotel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Swagger/OpenAPI (only for Development)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Infrastructure (Database + Repositories)
var connectionString = GetConnectionString(builder.Configuration);
builder.Services.AddInfrastructure(connectionString);

// Configure Application Services
builder.Services.AddApplication();

var app = builder.Build();

// Apply migrations and seed data in Development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    await context.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(context);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hotel Reservation API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Helper method to build connection string from environment variables or configuration
static string GetConnectionString(IConfiguration configuration)
{
    // First, try to get full connection string from environment variable
    var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
    if (!string.IsNullOrEmpty(connectionString))
    {
        return connectionString;
    }

    // Try to build connection string from individual environment variables
    var host = Environment.GetEnvironmentVariable("DATABASE_HOST");
    if (!string.IsNullOrEmpty(host))
    {
        var port = Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "5432";
        var database = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "hotel_db";
        var user = Environment.GetEnvironmentVariable("DATABASE_USER") ?? "postgres";
        var password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "postgres";

        return $"Host={host};Port={port};Database={database};Username={user};Password={password}";
    }

    // Fall back to configuration file
    return configuration.GetConnectionString("DefaultConnection")
           ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

// Make Program class accessible for integration tests
public partial class Program { }
