using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PharmacyInventory.API.Middleware;
using PharmacyInventory.Application.Interfaces;
using PharmacyInventory.Application.Services;
using PharmacyInventory.Application.Validators;
using PharmacyInventory.Infrastructure.Persistence;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ---- Services -------------------------------------------------------------

builder.Services.AddControllers();

builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PharmacyInventory API", Version = "v1" });

    // JWT Bearer auth for Swagger (adds Authorize button and sends Authorization header)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                Scheme = "bearer",
                Name = "Authorization",
                In = ParameterLocation.Header
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddValidatorsFromAssemblyContaining<CreateMedicineDtoValidator>();

builder.Services.AddSingleton<IMedicineRepository, JsonMedicineRepository>();
builder.Services.AddScoped<IMedicineService, MedicineService>();

const string AngularClientPolicy = "AngularClient";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularClientPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// --- JWT Authentication ----------------------------------------------------
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("JWT key missing in configuration.");
var issuer = jwtSection["Issuer"];
var audience = jwtSection["Audience"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = !string.IsNullOrEmpty(issuer),
            ValidateAudience = !string.IsNullOrEmpty(audience),
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            RoleClaimType = ClaimTypes.Role
        };

        // Diagnostic logging for authentication failures (temporary for debugging)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtAuth");

                var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

                logger.LogWarning("AUTH HEADER: {AuthHeader}", authHeader);
                logger.LogWarning("JWT TOKEN: {Token}", context.Token);

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtAuth");
                logger.LogError(context.Exception, "JWT authentication failed");

                Console.WriteLine("========== JWT AUTH FAILED ==========");
                Console.WriteLine(context.Exception.ToString());
                Console.WriteLine("=====================================");

                // Do not put raw exception text into headers (may contain newlines/control chars).
                // For local debugging, include a sanitized single-line summary.
                if (!context.HttpContext.Response.HasStarted)
                {
                    var msg = context.Exception?.Message ?? "unknown";
                    // Remove CR/LF and other control characters to make header safe
                    var safe = new string(msg.Where(c => c >= 0x20 && c <= 0x7E).ToArray());
                    if (string.IsNullOrWhiteSpace(safe))
                    {
                        safe = "auth-failed";
                    }
                    context.Response.Headers["X-Auth-Failure"] = safe;
                }

                return Task.CompletedTask;
            }
        };
    });

// --- Rate limiting (per-client IP) -----------------------------------------
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(clientId, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100, // 100 requests
            Window = TimeSpan.FromMinutes(1), // per minute
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.Headers.RetryAfter = "60";
        await context.HttpContext.Response.WriteAsync("Too many requests", token);
    };
});

var app = builder.Build();

// ---- Middleware pipeline --------------------------------------------------

// Must be first so it can catch exceptions thrown by anything further down the pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Security headers early in the pipeline
app.UseMiddleware<SecurityHeadersMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(AngularClientPolicy);

// Rate limiter middleware
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
