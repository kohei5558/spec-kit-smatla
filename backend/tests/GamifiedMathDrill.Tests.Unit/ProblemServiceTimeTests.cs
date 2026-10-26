using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Core.Services;
using Moq;
using Xunit;

namespace GamifiedMathDrill.Tests.Unit;

/// <summary>
/// 回答の日時は TimeProvider から取る（「今日の◯問め」や「◯日連続」と同じ時計を使う）
/// </summary>
public class ProblemServiceTimeTests
{
    [Fact]
    public async Task SubmitAnswer_RecordsSolvedAtFromTimeProvider()
    {
        var now = new DateTimeOffset(2026, 10, 10, 14, 59, 0, TimeSpan.Zero); // 日本時間 10/10 23:59
        var problem = new Problem { Id = 1, CalculationType = CalculationType.Addition, DifficultyLevel = 1, CorrectAnswer = 3 };
        var student = new Student { Id = 7 };

        var problems = new Mock<IProblemRepository>();
        problems.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(problem);
        var students = new Mock<IStudentRepository>();
        students.Setup(r => r.GetByIdWithDetailsAsync(7)).ReturnsAsync(student);
        var records = new Mock<ILearningRecordRepository>();
        LearningRecord? saved = null;
        records.Setup(r => r.AddAsync(It.IsAny<LearningRecord>())).Callback<LearningRecord>(r => saved = r).ReturnsAsync((LearningRecord r) => r);
        var levels = new Mock<ILevelRepository>();
        levels.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int id) => new Level { Id = id, LevelNumber = id, RequiredCorrectAnswers = 5, MinDifficulty = 1, MaxDifficulty = 2 });

        var service = new ProblemService(problems.Object, students.Object, records.Object, levels.Object, new FixedTimeProvider(now));
        await service.SubmitAnswerAsync(7, 1, 3);

        Assert.Equal(now.UtcDateTime, saved!.SolvedAt);
        Assert.Equal(DateTimeKind.Utc, saved.SolvedAt.Kind);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
