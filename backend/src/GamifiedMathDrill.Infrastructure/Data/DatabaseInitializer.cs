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
    /// <param name="seedDemoUsers">既知のパスワードを持つデモ用ユーザーを作成するか（開発環境のみtrueにすること）</param>
    public static async Task InitializeDatabaseAsync(this ApplicationDbContext context, IServiceProvider? serviceProvider = null, bool seedDemoUsers = false)
    {
        // マイグレーションを適用
        await context.Database.MigrateAsync();

        // プリセットアバターデータを追加
        await AvatarSeeder.SeedAsync(context);

        // レベル・問題（未投入のときだけ）
        if (!await context.Levels.AnyAsync())
        {
            var levels = LevelSeeder.GetLevels();
            await context.Levels.AddRangeAsync(levels);
            await context.SaveChangesAsync();

            var problems = ProblemSeeder.GetProblems();
            await context.Problems.AddRangeAsync(problems);
            await context.SaveChangesAsync();
        }

        // 景品は家庭ごとに持つため、ここでは作らない（保護者の登録時に初期景品をコピーする）

        // デモユーザー（Identity使用のため serviceProvider 経由）。学習者がレベルを参照するため、レベルの投入後に行う
        if (seedDemoUsers && serviceProvider != null)
        {
            await UserSeeder.SeedUsersAsync(serviceProvider);
        }
    }
}
