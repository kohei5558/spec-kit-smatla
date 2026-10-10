using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using GamifiedMathDrill.Infrastructure.Data;
using Microsoft.Extensions.Hosting;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// 統合テスト用のWebApplicationFactory
/// インメモリデータベースを使用してテストを実行
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// テスト用保護者（parent@example.com）が登録した端末のトークン。端末の上限に達しないよう、クラス内で使い回す
    /// </summary>
    public string? ParentDeviceToken { get; set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 既存のDbContext設定を削除
            // EF Core 9以降はプロバイダー設定が IDbContextOptionsConfiguration<T> にも登録されるため、両方を外す
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();

            // インメモリデータベースを使用するDbContextを追加（各テストで独立したDB）
            var dbName = $"TestDb_{Guid.NewGuid()}";
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(dbName);
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
            // 本番と同じレベル（1〜10）を使う（学年に応じた始めるレベルを確認するため）
            db.Levels.AddRange(GamifiedMathDrill.Infrastructure.Data.Seed.LevelSeeder.GetLevels());
            db.SaveChanges();
        }

        // プリセットアバターデータをシード
        if (!db.PresetAvatars.Any())
        {
            db.PresetAvatars.AddRange(
                new GamifiedMathDrill.Core.Models.PresetAvatar { Name = "猫", FileName = "cat.png", DisplayOrder = 1 },
                new GamifiedMathDrill.Core.Models.PresetAvatar { Name = "犬", FileName = "dog.png", DisplayOrder = 2 },
                new GamifiedMathDrill.Core.Models.PresetAvatar { Name = "パンダ", FileName = "panda.png", DisplayOrder = 3 },
                new GamifiedMathDrill.Core.Models.PresetAvatar { Name = "ライオン", FileName = "lion.png", DisplayOrder = 4 },
                new GamifiedMathDrill.Core.Models.PresetAvatar { Name = "ウサギ", FileName = "rabbit.png", DisplayOrder = 5 }
            );
            db.SaveChanges();
        }

        // 問題は本番と同じ ProblemGenerator で作る（難易度表どおりの問題・あまりのあるわり算を含む）
        if (!db.Problems.Any())
        {
            db.Problems.AddRange(GamifiedMathDrill.Core.Services.ProblemGenerator.GenerateAll());
            db.SaveChanges();
        }

        return host;
    }
}
