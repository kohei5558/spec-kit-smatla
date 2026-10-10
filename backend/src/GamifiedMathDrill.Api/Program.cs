using GamifiedMathDrill.Api.Middleware;
using GamifiedMathDrill.Api.Services;
using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Services;
using GamifiedMathDrill.Infrastructure.Data;
using GamifiedMathDrill.Infrastructure.Identity;
using GamifiedMathDrill.Infrastructure.Jobs;
using GamifiedMathDrill.Infrastructure.Repositories;
using GamifiedMathDrill.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

// Serilogの設定
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/gamifiedmathdrill-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Serilogを使用
builder.Host.UseSerilog();

// Add services to the container.
// DB の種類は設定 DatabaseProvider（Sqlite / PostgreSQL）で指定。未指定なら開発は SQLite、それ以外は PostgreSQL。
// マイグレーションは DB の種類ごとに別プロジェクトに置いている（型やシーケンスの書き方が異なるため）
var databaseProvider = builder.Configuration["DatabaseProvider"]
    ?? (builder.Environment.IsDevelopment() ? "Sqlite" : "PostgreSQL");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    switch (databaseProvider)
    {
        case "Sqlite":
            options.UseSqlite(connectionString, b => b.MigrationsAssembly("GamifiedMathDrill.Migrations.Sqlite"));
            break;
        case "PostgreSQL":
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly("GamifiedMathDrill.Migrations.PostgreSQL"));
            break;
        default:
            throw new InvalidOperationException($"Unknown DatabaseProvider: {databaseProvider}（Sqlite または PostgreSQL を指定してください）");
    }
});

// Register repositories
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IProblemRepository, ProblemRepository>();
builder.Services.AddScoped<ILearningRecordRepository, LearningRecordRepository>();
builder.Services.AddScoped<ILevelRepository, LevelRepository>();
builder.Services.AddScoped<IRewardRepository, RewardRepository>();
builder.Services.AddScoped<IAcquiredRewardRepository, AcquiredRewardRepository>();
builder.Services.AddScoped<IExchangeRequestRepository, ExchangeRequestRepository>();
builder.Services.AddScoped<IDailyChallengeRepository, DailyChallengeRepository>();
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
builder.Services.AddScoped<IPresetAvatarRepository, PresetAvatarRepository>();

// Register services
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IProblemService, ProblemService>();
builder.Services.AddScoped<ILearningRecordService, LearningRecordService>();
builder.Services.AddScoped<IDailyChallengeService, DailyChallengeService>();
builder.Services.AddScoped<IRewardService, RewardService>();
builder.Services.AddScoped<IExchangeRequestService, ExchangeRequestService>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IAuthService, GamifiedMathDrill.Infrastructure.Services.AuthService>();
builder.Services.AddScoped<IImageStorageService, ImageStorageService>();
builder.Services.AddScoped<IParentDashboardService, ParentDashboardService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IChildAccountService, ChildAccountService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IStudentAccessService, StudentAccessService>();

// Memory cache for PIN lockout
builder.Services.AddMemoryCache();

// Background services
builder.Services.AddHostedService<DailyChallengeJob>();

// ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "GamifiedMathDrill.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "GamifiedMathDrill.Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// 既定ですべてのAPIでログインを必須にする（未ログインで使うAPIは [AllowAnonymous] を明示する）
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Data Protection API (セッショントークン用)
builder.Services.AddDataProtection();

// CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:5001", "http://localhost:5000", "http://localhost:5071", "https://localhost:7071", "http://localhost:5002")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // ModelState自動バリデーションを無効化（サービス層でバリデーション実施）
        options.SuppressModelStateInvalidFilter = true;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Initialize database with seed data (テスト環境では実行しない)
if (app.Environment.EnvironmentName != "Testing")
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.InitializeDatabaseAsync(scope.ServiceProvider, seedDemoUsers: app.Environment.IsDevelopment());
    }
}

// Configure the HTTP request pipeline.
app.UseErrorHandling();

// Request/response logging for debugging tests (Testing environment only)
if (app.Environment.IsEnvironment("Testing"))
{
    app.Use(async (context, next) =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Incoming request {Method} {Path}", context.Request.Method, context.Request.Path);
        await next();
        logger.LogInformation("Outgoing response {StatusCode} for {Path} (Content-Length: {Length})", context.Response.StatusCode, context.Request.Path, context.Response.ContentLength);
    });
}

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    }

    await next();
});

// レート制限ミドルウェア（テスト環境では無効化）
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseMiddleware<RateLimitMiddleware>();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// In testing environment we avoid HTTPS redirection to prevent redirect responses
// (which can produce empty bodies in test clients when HTTPS port is not configured).
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

// Serve static files from wwwroot (uploads, etc.)
app.UseStaticFiles();

app.UseCors("AllowBlazorClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// テスト用にProgramクラスを公開
public partial class Program { }
