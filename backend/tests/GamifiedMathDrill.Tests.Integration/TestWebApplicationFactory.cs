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

        // テスト用の問題をシード
        if (!db.Problems.Any())
        {
            var problems = new List<GamifiedMathDrill.Core.Models.Problem>();
            var types = new[] { 
                GamifiedMathDrill.Core.Models.CalculationType.Addition,
                GamifiedMathDrill.Core.Models.CalculationType.Subtraction,
                GamifiedMathDrill.Core.Models.CalculationType.Multiplication,
                GamifiedMathDrill.Core.Models.CalculationType.Division
            };

            // 各カテゴリ、各難易度に10問ずつ作成（合計400問）
            foreach (var type in types)
            {
                for (int difficulty = 1; difficulty <= 10; difficulty++)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        int num1 = 0, num2 = 0, answer = 0;
                        string question = "";

                        switch (type)
                        {
                            case GamifiedMathDrill.Core.Models.CalculationType.Addition:
                                num1 = difficulty * (i + 1);
                                num2 = difficulty * (i + 2);
                                answer = num1 + num2;
                                question = $"{num1} + {num2} = ?";
                                break;
                            case GamifiedMathDrill.Core.Models.CalculationType.Subtraction:
                                num1 = difficulty * (i + 3);
                                num2 = difficulty * (i + 1);
                                answer = num1 - num2;
                                question = $"{num1} - {num2} = ?";
                                break;
                            case GamifiedMathDrill.Core.Models.CalculationType.Multiplication:
                                num1 = difficulty + i;
                                num2 = difficulty;
                                answer = num1 * num2;
                                question = $"{num1} × {num2} = ?";
                                break;
                            case GamifiedMathDrill.Core.Models.CalculationType.Division:
                                num2 = difficulty + i + 1;
                                answer = difficulty + i;
                                num1 = num2 * answer;
                                question = $"{num1} ÷ {num2} = ?";
                                break;
                        }

                        problems.Add(new GamifiedMathDrill.Core.Models.Problem
                        {
                            Question = question,
                            CorrectAnswer = answer,
                            DifficultyLevel = difficulty,
                            CalculationType = type
                        });
                    }
                }
            }

            db.Problems.AddRange(problems);
            db.SaveChanges();
        }

        return host;
    }
}
