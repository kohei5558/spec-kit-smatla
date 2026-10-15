using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Core.Services;
using GamifiedMathDrill.Infrastructure.Data;
using GamifiedMathDrill.Infrastructure.Data.Seed;
using GamifiedMathDrill.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// 起動時に問題を最新のセットに合わせる処理のテスト（011）
/// </summary>
public class ProblemSynchronizerTests
{
    private static ApplicationDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase($"Sync_{Guid.NewGuid()}").Options);

    private static Problem P(string question, int answer, CalculationType type = CalculationType.Addition, int difficulty = 1, int? remainder = null) =>
        new() { Question = question, CorrectAnswer = answer, CorrectRemainder = remainder, CalculationType = type, DifficultyLevel = difficulty };

    [Fact]
    public async Task EmptyDb_GetsAllProblems_AndSecondRunChangesNothing()
    {
        using var db = CreateContext();
        var desired = ProblemGenerator.GenerateAll();

        var first = await ProblemSynchronizer.SyncAsync(db, desired);
        var idsAfterFirst = await db.Problems.OrderBy(p => p.Id).Select(p => p.Id).ToListAsync();
        var second = await ProblemSynchronizer.SyncAsync(db, ProblemGenerator.GenerateAll());

        Assert.Equal(desired.Count, first.Added);
        Assert.Equal(desired.Count, await db.Problems.CountAsync(p => p.IsActive));
        Assert.Equal(new ProblemSyncResult(0, 0, 0, 0), second);
        Assert.Equal(idsAfterFirst, await db.Problems.OrderBy(p => p.Id).Select(p => p.Id).ToListAsync());
    }

    [Fact]
    public async Task OldProblem_IsDeactivated_AndItsLearningRecordsRemain()
    {
        using var db = CreateContext();
        var old = P("10 × 10 = ?", 100, CalculationType.Multiplication, 6);
        var kept = P("1 + 1 = ?", 2);
        db.Problems.AddRange(old, kept);
        await db.SaveChangesAsync();
        db.LearningRecords.Add(new LearningRecord { StudentId = 1, ProblemId = old.Id, StudentAnswer = 100, IsCorrect = true, PointsEarned = 10 });
        await db.SaveChangesAsync();

        var result = await ProblemSynchronizer.SyncAsync(db, new List<Problem> { P("1 + 1 = ?", 2), P("2 + 3 = ?", 5) });

        Assert.Equal(new ProblemSyncResult(Added: 1, Deactivated: 1, Reactivated: 0, Corrected: 0), result);
        Assert.False((await db.Problems.FindAsync(old.Id))!.IsActive);
        Assert.True((await db.Problems.FindAsync(kept.Id))!.IsActive);
        Assert.Equal(1, await db.LearningRecords.CountAsync(r => r.ProblemId == old.Id));
        Assert.Equal(3, await db.Problems.CountAsync()); // 削除はしない
    }

    [Fact]
    public async Task InactiveProblem_BackInSet_IsReactivatedWithSameId()
    {
        using var db = CreateContext();
        var problem = P("4 + 5 = ?", 9);
        problem.IsActive = false;
        db.Problems.Add(problem);
        await db.SaveChangesAsync();

        var result = await ProblemSynchronizer.SyncAsync(db, new List<Problem> { P("4 + 5 = ?", 9) });

        Assert.Equal(new ProblemSyncResult(0, 0, Reactivated: 1, 0), result);
        Assert.Equal(problem.Id, (await db.Problems.SingleAsync()).Id);
        Assert.True((await db.Problems.SingleAsync()).IsActive);
    }

    [Fact]
    public async Task SameQuestion_WithDifferentDifficulty_IsTreatedAsNewProblem()
    {
        using var db = CreateContext();
        var old = P("10 × 10 = ?", 100, CalculationType.Multiplication, 6);
        db.Problems.Add(old);
        await db.SaveChangesAsync();

        var result = await ProblemSynchronizer.SyncAsync(db, new List<Problem> { P("10 × 10 = ?", 100, CalculationType.Multiplication, 10) });

        Assert.Equal(new ProblemSyncResult(Added: 1, Deactivated: 1, 0, 0), result);
        Assert.False((await db.Problems.FindAsync(old.Id))!.IsActive);
        Assert.True(await db.Problems.AnyAsync(p => p.DifficultyLevel == 10 && p.IsActive));
    }

    [Fact]
    public async Task WrongAnswer_IsCorrected_AndDuplicatesAreDeactivated()
    {
        using var db = CreateContext();
        var wrong = P("17 ÷ 5 = ?", 3, CalculationType.DivisionWithRemainder, 2, remainder: 1);
        var duplicate = P("17 ÷ 5 = ?", 3, CalculationType.DivisionWithRemainder, 2, remainder: 2);
        db.Problems.AddRange(wrong, duplicate);
        await db.SaveChangesAsync();

        var result = await ProblemSynchronizer.SyncAsync(db, new List<Problem> { P("17 ÷ 5 = ?", 3, CalculationType.DivisionWithRemainder, 2, remainder: 2) });

        // 先に作られた方（ID が小さい方）を使い、正解を直す。重なった方は使わない
        Assert.Equal(new ProblemSyncResult(0, Deactivated: 1, 0, Corrected: 1), result);
        var active = await db.Problems.SingleAsync(p => p.IsActive);
        Assert.Equal(wrong.Id, active.Id);
        Assert.Equal(2, active.CorrectRemainder);
    }

    [Fact]
    public async Task RandomProblem_NeverReturnsInactiveProblem()
    {
        using var db = CreateContext();
        var inactive = P("9 + 9 = ?", 18, difficulty: 3);
        inactive.IsActive = false;
        db.Problems.AddRange(inactive, P("8 + 8 = ?", 16, difficulty: 3));
        await db.SaveChangesAsync();
        var repository = new ProblemRepository(db);

        for (var i = 0; i < 30; i++)
        {
            Assert.NotEqual(inactive.Id, (await repository.GetRandomProblemAsync(3))!.Id);
        }

        // 使う問題が残っていなければ null（使わない問題は選ばない）
        Assert.Null(await repository.GetRandomProblemAsync(3, excludeIds: (await db.Problems.Where(p => p.IsActive).Select(p => p.Id).ToListAsync())));
    }
}
