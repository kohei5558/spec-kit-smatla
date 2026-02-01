using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using GamifiedMathDrill.Infrastructure.Data;
using Microsoft.Extensions.Hosting;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// 統合テスト用のWebApplicationFactory
/// インメモリデータベースを使用してテストを実行
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 既存のDbContextを削除
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // インメモリデータベースを使用するDbContextを追加
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            // DailyChallengeJobを削除（テスト環境では不要）
            var hostedService = services.SingleOrDefault(
                d => d.ImplementationType?.Name == "DailyChallengeJob");
            if (hostedService != null)
            {
                services.Remove(hostedService);
            }
        });

        // テスト用のJWT SecretKeyを設定
        builder.UseSetting("Jwt:SecretKey", "ThisIsATestSecretKeyForIntegrationTestsWithMinimum32Characters!");
        
        // テスト環境として設定
        builder.UseEnvironment("Testing");
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // データベース初期化とシード
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
        
        // 初期データのシード（Levelsテーブルなど）
        if (!db.Levels.Any())
        {
            db.Levels.AddRange(
                new GamifiedMathDrill.Core.Models.Level { LevelNumber = 1, MinDifficulty = 1, MaxDifficulty = 3 },
                new GamifiedMathDrill.Core.Models.Level { LevelNumber = 2, MinDifficulty = 4, MaxDifficulty = 6 },
                new GamifiedMathDrill.Core.Models.Level { LevelNumber = 3, MinDifficulty = 7, MaxDifficulty = 10 }
            );
            db.SaveChanges();
        }

        return host;
    }
}
