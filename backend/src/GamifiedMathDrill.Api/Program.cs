using GamifiedMathDrill.Api.Middleware;
using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Services;
using GamifiedMathDrill.Infrastructure.Data;
using GamifiedMathDrill.Infrastructure.Repositories;
using GamifiedMathDrill.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

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
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (builder.Environment.IsDevelopment())
    {
        options.UseSqlite(connectionString);
    }
    else
    {
        options.UseNpgsql(connectionString);
    }
});

// Register repositories
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IProblemRepository, ProblemRepository>();
builder.Services.AddScoped<ILearningRecordRepository, LearningRecordRepository>();
builder.Services.AddScoped<ILevelRepository, LevelRepository>();
builder.Services.AddScoped<IRewardRepository, RewardRepository>();
builder.Services.AddScoped<IAcquiredRewardRepository, AcquiredRewardRepository>();
builder.Services.AddScoped<IDailyChallengeRepository, DailyChallengeRepository>();

// Register services
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IProblemService, ProblemService>();
builder.Services.AddScoped<ILearningRecordService, LearningRecordService>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();

// Data Protection API (セッショントークン用)
builder.Services.AddDataProtection();

// CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:5001", "http://localhost:5000", "http://localhost:5071", "https://localhost:7071")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Initialize database with seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.InitializeDatabaseAsync();
}

// Configure the HTTP request pipeline.
app.UseErrorHandling();

// レート制限ミドルウェア
app.UseRateLimiting();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowBlazorClient");

app.UseAuthorization();

app.MapControllers();

app.Run();
