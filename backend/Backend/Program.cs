using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Microsoft.OpenApi.Models;

/// <summary>
/// ESG PLATFORM STARTUP CONFIGURATION
/// 
/// This file configures the entire ASP.NET Core application:
/// 1. Database connection (PostgreSQL with Entity Framework)
/// 2. Dependency injection services
/// 3. HTTP request pipeline
/// 4. API controllers and routing
/// 
/// Key Features:
/// - Entity Framework Core with PostgreSQL
/// - Controller-based API endpoints
/// - Development and production configurations
/// - Database migration support
/// </summary>

var builder = WebApplication.CreateBuilder(args);

// =============================================================================
// DATABASE CONFIGURATION
// =============================================================================

// Add PostgreSQL database context with connection string from appsettings.json
builder.Services.AddDbContext<ESGDbContext>(options =>
{
    // Get connection string from appsettings.json
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Configure PostgreSQL with Entity Framework
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        // Enable retry on failure for better reliability
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    });
    
    // Enable detailed errors in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// =============================================================================
// API SERVICES CONFIGURATION
// =============================================================================

// Add API controllers support
builder.Services.AddControllers();

// Add API documentation (Swagger/OpenAPI)
builder.Services.AddOpenApi();
// Add Swagger services for UI
builder.Services.AddSwaggerGen();

// Add CORS for frontend integration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // React dev server
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// =============================================================================
// BUILD APPLICATION
// =============================================================================

var app = builder.Build();

// =============================================================================
// DEVELOPMENT CONFIGURATION
// =============================================================================

if (app.Environment.IsDevelopment())
{
    // Enable API documentation
    app.MapOpenApi();
    // Enable Swagger UI for endpoint testing
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ESG Platform API v1");
        c.RoutePrefix = "swagger";
    });
    
    // Temporarily disable auto-migration to troubleshoot startup
    // TODO: Re-enable after confirming server starts properly
    /*
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ESGDbContext>();
            
            // Ensure database is created and migrations are applied
            dbContext.Database.Migrate();
            
            Console.WriteLine("✅ Database migrations applied successfully");
            
            // Seed initial data for development and testing
            await DatabaseSeeder.SeedAsync(dbContext);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Database migration failed: {ex.Message}");
            Console.WriteLine("Make sure PostgreSQL is running and connection string is correct");
        }
    }
    */
}

// =============================================================================
// HTTP PIPELINE CONFIGURATION
// =============================================================================

// Enable CORS
app.UseCors("AllowFrontend");

// Enable HTTPS redirection
app.UseHttpsRedirection();

// Enable controller routing
app.MapControllers();

// =============================================================================
// HEALTH CHECK ENDPOINT
// =============================================================================

// Simple health check to verify API is running
app.MapGet("/health", async (ESGDbContext dbContext) =>
{
    try
    {
        // Test database connectivity
        await dbContext.Database.CanConnectAsync();
        
        return Results.Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Database = "Connected",
            Environment = app.Environment.EnvironmentName
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            title: "Database Connection Failed",
            statusCode: 503
        );
    }
})
.WithName("HealthCheck");

// =============================================================================
// START APPLICATION
// =============================================================================

Console.WriteLine("🚀 Starting ESG Platform API...");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine("📊 Framework tables ready for data ingestion");

app.Run();
