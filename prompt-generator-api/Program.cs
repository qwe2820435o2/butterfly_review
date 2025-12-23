using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using MongoDB.Driver;
using tennis_wave_api.Data;
using tennis_wave_api.Extensions;
using tennis_wave_api.Models;
using tennis_wave_api.Models.Entities;

var myAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

// Serilog Config
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Set up JWT with environment variable support
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null)
{
    jwtSettings = new JwtSettings
    {
        SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? "your-super-secret-key-here-make-it-long-and-random-at-least-32-characters",
        Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "prompt-generator-api",
        Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "tennis-wave-users",
        ExpiryInMinutes = 60
    };
}
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings?.Issuer,
            ValidAudience = jwtSettings?.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ?? string.Empty))
        };
        
    });

// Enhance Swagger Config
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Prompt Generator API",
        Version = "v1.0.0",
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        // A short description for the authentication scheme.
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        // The name of the header that will contain the JWT.
        Name = "Authorization",
        // The location of the API key.
        In = ParameterLocation.Header,
        // The type of the security scheme.
        Type = SecuritySchemeType.Http, 
        // The name of the HTTP authorization scheme.
        Scheme = "bearer", 
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Add services to the container.

// Configure MongoDB settings
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection(MongoDbSettings.SectionName));

// Get MongoDB connection string from multiple sources
var mongoDbSettings = builder.Configuration.GetSection(MongoDbSettings.SectionName).Get<MongoDbSettings>();
var mongoConnection = string.Empty;

// Priority 1: Check MongoDb:ConnectionString configuration
if (!string.IsNullOrWhiteSpace(mongoDbSettings?.ConnectionString))
{
    mongoConnection = mongoDbSettings.ConnectionString;
    Log.Information("MongoDB connection string found in MongoDb:ConnectionString configuration");
}
// Priority 2: Check environment variable MongoDb__ConnectionString
else if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MongoDb__ConnectionString")))
{
    mongoConnection = Environment.GetEnvironmentVariable("MongoDb__ConnectionString")!;
    Log.Information("MongoDB connection string found in MongoDb__ConnectionString environment variable");
}
// Priority 3: Check ConnectionStrings__DefaultConnection environment variable
else if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")))
{
    var defaultConnection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
    if (!string.IsNullOrWhiteSpace(defaultConnection) && defaultConnection.StartsWith("mongodb", StringComparison.OrdinalIgnoreCase))
    {
        mongoConnection = defaultConnection;
        Log.Information("MongoDB connection string found in ConnectionStrings__DefaultConnection environment variable");
    }
}
// Priority 4: Check DefaultConnection from configuration if it's a MongoDB connection string
else
{
    var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrWhiteSpace(defaultConnection) && defaultConnection.StartsWith("mongodb", StringComparison.OrdinalIgnoreCase))
    {
        mongoConnection = defaultConnection;
        Log.Information("MongoDB connection string found in DefaultConnection configuration");
    }
}

// Always register MongoDB client and helper to avoid dependency injection errors
if (!string.IsNullOrWhiteSpace(mongoConnection) && mongoConnection.StartsWith("mongodb", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnection));
    builder.Services.AddScoped<MongoDbHelper>(provider => 
        new MongoDbHelper(provider.GetRequiredService<IMongoClient>(), mongoDbSettings ?? new MongoDbSettings()));
    
    Log.Information("MongoDB client registered successfully. Connection string: {ConnectionString}", 
        mongoConnection.Substring(0, Math.Min(30, mongoConnection.Length)) + "...");
}
else
{
    // Register with a placeholder connection string to avoid DI errors
    // This will allow the app to start, but MongoDB operations will fail at runtime
    Log.Warning("⚠️ MongoDB connection string not found! Using placeholder connection. " +
                "Please configure one of the following environment variables: " +
                "MongoDb__ConnectionString or ConnectionStrings__DefaultConnection. " +
                "MongoDB operations will fail until connection string is configured.");
    
    // Use a placeholder that will fail at runtime with a clear error message
    builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient("mongodb://placeholder:27017"));
    builder.Services.AddScoped<MongoDbHelper>(provider => 
        new MongoDbHelper(provider.GetRequiredService<IMongoClient>(), mongoDbSettings ?? new MongoDbSettings()));
    
    Log.Warning("MongoDB services registered with placeholder connection. Configure connection string to enable MongoDB operations.");
}

// Add Controllers
builder.Services.AddControllers();

// Add Services
builder.Services.AddApplicationServices();

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));


// Cors 
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
        policy =>
        {
            policy
                .SetIsOriginAllowed(_ => true) // Allow any origin but specify explicitly
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials(); // Important
        });
});

builder.Services.Configure<JotformSettings>(
    builder.Configuration.GetSection(JotformSettings.SectionName));

builder.Services.AddHttpClient();


var app = builder.Build();

// Show current info
var env = builder.Environment.EnvironmentName;
var appName = builder.Configuration["AppSettings:AppName"] ?? "Prompt Generator API";
var version = builder.Configuration["AppSettings:Version"] ?? "1.0.0";

Console.WriteLine("=".PadRight(60, '='));
Console.WriteLine($"Starting {appName} v{version}");
Console.WriteLine($"Environment: {env}");
Console.WriteLine($"Database: {builder.Configuration.GetConnectionString("DefaultConnection")?.Split(';').FirstOrDefault()?.Split('=').LastOrDefault() ?? "Not configured"}");
Console.WriteLine($"Log Level: {builder.Configuration["Logging:LogLevel:Default"] ?? "Information"}");
Console.WriteLine("=".PadRight(60, '='));

// Initialize MongoDB indexes synchronously on startup
if (!string.IsNullOrWhiteSpace(mongoConnection) && mongoConnection.StartsWith("mongodb", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        Console.WriteLine("Initializing MongoDB indexes...");
        using var scope = app.Services.CreateScope();
        var dbHelper = scope.ServiceProvider.GetRequiredService<MongoDbHelper>();
        var releaseRepo = scope.ServiceProvider.GetRequiredService<tennis_wave_api.Data.Interfaces.IReleaseSubmissionRepository>();
        var sightingRepo = scope.ServiceProvider.GetRequiredService<tennis_wave_api.Data.Interfaces.ISightingSubmissionRepository>();
        
        // Force index creation by accessing the repositories (they will create indexes in constructor)
        // Wait a bit to ensure async index creation completes
        await Task.Delay(2000);
        
        Console.WriteLine("MongoDB indexes initialized successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Failed to initialize MongoDB indexes: {ex.Message}");
        // Don't fail startup if index creation fails
    }
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Prompt Generator API v1");
        c.DocumentTitle = "Prompt Generator API Documentation";
    });
    app.UseDeveloperExceptionPage();
}
else
{   
    app.UseMiddleware<ExceptionHandlerMiddleware>();
}


// Cors - Must be before authentication and authorization
app.UseCors(myAllowSpecificOrigins);

// Add CORS debugging middleware
app.Use(async (context, next) =>
{
    var origin = context.Request.Headers["Origin"].ToString();
    var method = context.Request.Method;
    var path = context.Request.Path;
    var userAgent = context.Request.Headers["User-Agent"].ToString();
    
    Console.WriteLine($"🔍 CORS Debug: Request from origin: {origin}");
    Console.WriteLine($"🔍 CORS Debug: Request method: {method}");
    Console.WriteLine($"🔍 CORS Debug: Request path: {path}");
    Console.WriteLine($"🔍 CORS Debug: User-Agent: {userAgent}");
    
    await next();
    
    // Log response headers
    Console.WriteLine($"🔍 CORS Debug: Response status: {context.Response.StatusCode}");
    Console.WriteLine($"🔍 CORS Debug: Access-Control-Allow-Origin: {context.Response.Headers["Access-Control-Allow-Origin"]}");
    Console.WriteLine($"🔍 CORS Debug: Access-Control-Allow-Credentials: {context.Response.Headers["Access-Control-Allow-Credentials"]}");
});

app.UseHttpsRedirection();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();


// Add Router
app.MapControllers();

// Configure port for Railway deployment
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    app.Urls.Add($"http://0.0.0.0:{port}");
}

app.Run();
