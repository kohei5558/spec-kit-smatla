using GamifiedMathDrill.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GamifiedMathDrill.Infrastructure.Data;

/// <summary>
/// データベース初期化の拡張メソッド
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// データベースをマイグレーションしてシードデータを投入
    /// </summary>
    public static async Task InitializeDatabaseAsync(this ApplicationDbContext context, IServiceProvider? serviceProvider = null)
    {
        // マイグレーションを適用
        await context.Database.MigrateAsync();

        // ユーザーデータを追加（Identity使用のため、serviceProvider経由で呼び出す）
        if (serviceProvider != null)
        {
            await UserSeeder.SeedUsersAsync(serviceProvider);
        }

        // プリセットアバターデータを追加
        await AvatarSeeder.SeedAsync(context);

        // シードデータが既に存在するかチェック
        if (await context.Levels.AnyAsync())
        {
            return; // 既にデータがあれば何もしない
        }

        // レベルデータを追加
        var levels = LevelSeeder.GetLevels();
        await context.Levels.AddRangeAsync(levels);
        await context.SaveChangesAsync();

        // 問題データを追加
        var problems = ProblemSeeder.GetProblems();
        await context.Problems.AddRangeAsync(problems);
        await context.SaveChangesAsync();

        // 景品データを追加
        var rewards = RewardSeeder.GetRewards();
        await context.Rewards.AddRangeAsync(rewards);
        await context.SaveChangesAsync();
    }
}
