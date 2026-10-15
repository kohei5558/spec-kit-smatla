using GamifiedMathDrill.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GamifiedMathDrill.Infrastructure.Data.Seed;

public record ProblemSyncResult(int Added, int Deactivated, int Reactivated, int Corrected);

/// <summary>
/// DB の問題を最新の問題セットに合わせる（011）。
/// 問題は学習記録・デイリーチャレンジが参照しているため削除せず、セットにない問題は使わない状態（IsActive = false）にする
/// </summary>
public static class ProblemSynchronizer
{
    public static async Task<ProblemSyncResult> SyncAsync(ApplicationDbContext context, IReadOnlyList<Problem> desired)
    {
        // 同じ問題かどうかは「計算の種類・難易度・問題文」で判断する。同じものが複数あれば先に作られた方（ID が小さい方）を使う
        var existing = await context.Problems.OrderBy(p => p.Id).ToListAsync();
        var byKey = existing
            .GroupBy(Key)
            .ToDictionary(g => g.Key, g => g.First());
        var desiredKeys = new HashSet<(CalculationType, int, string)>();
        int added = 0, reactivated = 0, corrected = 0;

        foreach (var problem in desired)
        {
            var key = Key(problem);
            if (!desiredKeys.Add(key))
            {
                continue;
            }

            if (!byKey.TryGetValue(key, out var current))
            {
                context.Problems.Add(new Problem
                {
                    Question = problem.Question,
                    CorrectAnswer = problem.CorrectAnswer,
                    CorrectRemainder = problem.CorrectRemainder,
                    CalculationType = problem.CalculationType,
                    DifficultyLevel = problem.DifficultyLevel,
                    IsActive = true
                });
                added++;
                continue;
            }

            if (!current.IsActive)
            {
                current.IsActive = true;
                reactivated++;
            }

            // 作り方の誤りを直したとき（同じ問題で正解が違う）は、正解を最新のセットに合わせる
            if (current.CorrectAnswer != problem.CorrectAnswer || current.CorrectRemainder != problem.CorrectRemainder)
            {
                current.CorrectAnswer = problem.CorrectAnswer;
                current.CorrectRemainder = problem.CorrectRemainder;
                corrected++;
            }
        }

        // 最新のセットにない問題と、重なった問題は使わない状態にする
        var deactivated = 0;
        foreach (var problem in existing.Where(p => p.IsActive))
        {
            var key = Key(problem);
            if (!desiredKeys.Contains(key) || !ReferenceEquals(byKey[key], problem))
            {
                problem.IsActive = false;
                deactivated++;
            }
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }

        return new ProblemSyncResult(added, deactivated, reactivated, corrected);
    }

    private static (CalculationType, int, string) Key(Problem p) => (p.CalculationType, p.DifficultyLevel, p.Question);
}
