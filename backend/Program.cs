using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Targets;
using NLog.Web;
using TriviaApp.API.Data;
using TriviaApp.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add user secrets configuration (for Development environment)
// Note: User secrets are automatically loaded when UserSecretsId is set in .csproj,
// but we explicitly add it here for clarity and to ensure it works in all scenarios
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets(System.Reflection.Assembly.GetExecutingAssembly());
}

// Ensure wwwroot directory exists
var wwwrootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
if (!Directory.Exists(wwwrootPath))
{
    Directory.CreateDirectory(wwwrootPath);
}

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Trivia API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Keycloak JWT. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "https://trivia.slackersoftware.com")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Forwarded headers (when running behind a reverse proxy like Nginx)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Clear known networks/proxies so forwarded headers from the proxy are accepted
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Configure PostgreSQL Database
var connectionString = builder.Configuration.GetConnectionString("SlackerDB");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'SlackerDB' not found. " +
        "Please set it using user secrets: dotnet user-secrets set ConnectionStrings:SlackerDB \"<your-connection-string>\"");
}

builder.Services.AddDbContext<TriviaDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Shared Slacker identity registry (public.user_account / public.user_identity).
// Owned outside this application: never generate or run EF migrations for it.
builder.Services.AddDbContextFactory<SharedIdentityDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<CurrentAccountService>();

var logConfiguration = LogManager.Configuration ?? throw new InvalidOperationException("NLog configuration is not set.");
var logTarget = logConfiguration.FindTargetByName("database")
    ?? throw new InvalidOperationException("Database target 'database' is not configured.");
var dbLog = logTarget as DatabaseTarget
    ?? throw new InvalidOperationException("Log target is not of type DatabaseTarget.");
dbLog.ConnectionString = connectionString;

builder.Host.UseNLog();

// Authentication (Keycloak OIDC, Bearer validation only)
var keycloakAuthority = builder.Configuration["Keycloak:Authority"]
    ?? throw new InvalidOperationException("Keycloak:Authority is not set.");
var keycloakAudience = builder.Configuration["Keycloak:Audience"]
    ?? throw new InvalidOperationException("Keycloak:Audience is not set.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakAuthority;
        options.Audience = keycloakAudience;
        options.RequireHttpsMetadata = true;
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = keycloakAuthority,
            ValidateAudience = true,
            ValidAudience = keycloakAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            NameClaimType = "preferred_username"
        };
        // NOTE: JwtBearerOptions.Events is null by default (unlike
        // OpenIdConnectOptions), so it must be assigned before use.
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var accounts = context.HttpContext.RequestServices.GetRequiredService<CurrentAccountService>();
                var accountId = await accounts.GetOrCreateAccountIdAsync(context.Principal!);
                if (accountId is null)
                {
                    context.Fail("The Keycloak user could not be mapped to a local account.");
                    return;
                }
                foreach (var identity in context.Principal!.Identities)
                {
                    identity.AddClaim(new Claim(CurrentAccountService.LocalAccountClaimType, accountId));
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, accountId));
                }
            }
        };
    });

builder.Services.AddAuthorization();

// Register services
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IImageService, ImageService>();

var app = builder.Build();

// Apply forwarded headers early so the app sees the original scheme and client IP
app.UseForwardedHeaders();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

// Serve static files for images
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot")),
    RequestPath = ""
});

app.MapControllers();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application is starting...");

try
{
    app.Run();
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred while starting the application.");
}
finally
{
    logger.LogInformation("Application is shutting down...");
    LogManager.Shutdown();
}
