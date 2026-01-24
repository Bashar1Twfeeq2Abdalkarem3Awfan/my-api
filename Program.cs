using Microsoft.EntityFrameworkCore;
using MyAPIv3.Data;
using MyAPIv3.Models; // ✨ for User
using MyAPIv3.Services;
using Microsoft.AspNetCore.Identity; // ✨ for IPasswordHasher
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);

// ----------------------
// Configure Kestrel for Railway (PORT environment variable)
// ----------------------
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

// ----------------------
// Add services
// ----------------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // استخدام JsonNamingPolicy للتأكد من camelCase
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ----------------------
// Register Services
// ----------------------
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<BackupService>();
builder.Services.AddScoped<DatabaseSeeder>();

// ----------------------
// DbContext
// ----------------------
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(conn));

// ----------------------
// JWT Authentication
// ----------------------
// Try multiple environment variable naming patterns
var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
    ?? Environment.GetEnvironmentVariable("Jwt__SecretKey")
    ?? Environment.GetEnvironmentVariable("JwtSecretKey")
    ?? builder.Configuration["Jwt:SecretKey"] 
    ?? throw new Exception("JWT Secret Key not configured!");

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MyAPIv3";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "sass_bt_mobile";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,      // مؤقتاً لإختبار
        ValidateAudience = false,    // مؤقتاً لإختبار
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// Authorization
builder.Services.AddAuthorization();
// Note: Removed FallbackPolicy to allow Swagger access
// API endpoints are protected by [Authorize] or [RequirePermission] attributes

// ----------------------
// Rate Limiting
// ----------------------
builder.Configuration.AddJsonFile("appsettings.RateLimiting.json", optional: true, reloadOnChange: true);
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// ----------------------
// CORS - Environment-based configuration
// ----------------------
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() 
    ?? new[] { "*" };

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Contains("*"))
        {
            // Development: Allow all origins
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            // Production: Specific origins only
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

// ----------------------
// Build app
// ----------------------
var app = builder.Build();

// ----------------------
// Auto-apply migrations on startup (for Railway)
// ----------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        
        // Apply any pending migrations automatically
        if (context.Database.GetPendingMigrations().Any())
        {
            Console.WriteLine("Applying pending migrations...");
            context.Database.Migrate();
            Console.WriteLine("Migrations applied successfully!");
        }
        else
        {
            Console.WriteLine("Database is up to date.");
        }

        // ✨ تشغيل الـ Seeder لإنشاء البيانات الأولية
        var seeder = services.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
        // Don't throw - let the app start even if initialization fails
    }
}

// Swagger - enabled in all environments for Railway
app.UseSwagger();
app.UseSwaggerUI();

// HSTS for production
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    // Note: Railway handles HTTPS termination, so we don't use UseHttpsRedirection()
}

// Enable CORS
app.UseCors();

// ✨ Enable static files serving (for logos, etc.)
app.UseStaticFiles();

// Rate Limiting
app.UseIpRateLimiting();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Run app
app.Run();
