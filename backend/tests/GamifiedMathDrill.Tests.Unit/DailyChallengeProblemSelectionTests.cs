using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Core.Services;
using Moq;
using Xunit;

namespace GamifiedMathDrill.Tests.Unit;

/// <summary>
/// デイリーチャレンジの問題の選び方と日付のテスト（009・010）
/// </summary>
public class DailyChallengeProblemSelectionTests
{
    private const int StudentId = 7;

    [Fact]
    public async Task Challenge_UsesLevelMaxDifficulty_AndNeverRemainderOrInactiveProblem()
    {
        var problems = new List<Problem>
        {
            new() { Id = 1, CalculationType = CalculationType.DivisionWithRemainder, DifficultyLevel = 6, CorrectRemainder = 1 },
            new() { Id = 5, CalculationType = CalculationType.Addition, DifficultyLevel = 6, IsActive = false }, // 使わない問題（011）
            new() { Id = 2, CalculationType = CalculationType.Addition, DifficultyLevel = 5 },
            new() { Id = 3, CalculationType = CalculationType.Division, DifficultyLevel = 6 },
            new() { Id = 4, CalculationType = CalculationType.Multiplication, DifficultyLevel = 7 }
        };

        // 乱数で選ばれるため、何日分か作って、すべて「レベル5の最大難易度6・割り切れる問題」であることを確認する
        for (var day = 0; day < 20; day++)
        {
            var (service, _) = CreateService(problems, new DateTimeOffset(2026, 10, 1, 3, 0, 0, TimeSpan.Zero).AddDays(day));
            var challenge = await service.GetOrCreateTodaysChallengeAsync(StudentId);
            Assert.Equal(3, challenge!.ProblemId);
            Assert.Equal(StudentId, challenge.StudentId);
            Assert.Equal(30, challenge.BonusPoints);
        }
    }

    /// <summary>
    /// 種類を1つ選び、その種類のレベルの最大難易度から作る（014）
    /// </summary>
    [Fact]
    public async Task Challenge_UsesMaxDifficultyOfChosenTypesLevel()
    {
        var problems = new List<Problem>
        {
            new() { Id = 10, CalculationType = CalculationType.Addition, DifficultyLevel = 8 },
            new() { Id = 11, CalculationType = CalculationType.Addition, DifficultyLevel = 6 },
            new() { Id = 12, CalculationType = CalculationType.Division, DifficultyLevel = 6 },
            new() { Id = 13, CalculationType = CalculationType.Division, DifficultyLevel = 8 }
        };
        var levels = new Dictionary<CalculationType, int> { [CalculationType.Addition] = 7, [CalculationType.Division] = 5 };

        var selected = new HashSet<int>();
        for (var day = 0; day < 40; day++)
        {
            var (service, _) = CreateService(problems, new DateTimeOffset(2026, 10, 1, 3, 0, 0, TimeSpan.Zero).AddDays(day), levels);
            selected.Add((await service.GetOrCreateTodaysChallengeAsync(StudentId))!.ProblemId);
        }

        // たし算はレベル7の最大難易度8、わり算はレベル5の最大難易度6。かけ算・ひき算は問題が無いのでほかの種類になる
        Assert.Equal(new HashSet<int> { 10, 12 }, selected);
    }

    [Theory]
    [InlineData("2026-10-10T14:59:00Z", "2026-10-10")] // 日本時間 10/10 23:59
    [InlineData("2026-10-10T15:00:00Z", "2026-10-11")] // 日本時間 10/11 0:00
    [InlineData("2026-10-10T23:00:00Z", "2026-10-11")] // 日本時間 10/11 8:00（UTC ではまだ 10/10）
    public async Task Challenge_DateIsJapanTime(string utcNow, string expectedDate)
    {
        var problems = new List<Problem> { new() { Id = 3, CalculationType = CalculationType.Division, DifficultyLevel = 6 } };
        var (service, _) = CreateService(problems, DateTimeOffset.Parse(utcNow));

        var challenge = await service.GetOrCreateTodaysChallengeAsync(StudentId);

        Assert.Equal(DateOnly.Parse(expectedDate), challenge!.TargetDate);
    }

    [Fact]
    public async Task Challenge_IsNull_WhenNoProblemForDifficulty()
    {
        var problems = new List<Problem> { new() { Id = 1, CalculationType = CalculationType.Addition, DifficultyLevel = 1 } };
        var (service, repository) = CreateService(problems, DateTimeOffset.UtcNow);

        Assert.Null(await service.GetOrCreateTodaysChallengeAsync(StudentId));
        repository.Verify(r => r.AddOrGetExistingAsync(It.IsAny<DailyChallenge>()), Times.Never);
    }

    private static (DailyChallengeService, Mock<IDailyChallengeRepository>) CreateService(List<Problem> problems, DateTimeOffset now, Dictionary<CalculationType, int>? levels = null)
    {
        var problemRepository = new Mock<IProblemRepository>();
        problemRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(problems);
        var challengeRepository = new Mock<IDailyChallengeRepository>();
        challengeRepository.Setup(r => r.GetByStudentAndDateAsync(It.IsAny<int>(), It.IsAny<DateOnly>())).ReturnsAsync((DailyChallenge?)null);
        challengeRepository.Setup(r => r.AddOrGetExistingAsync(It.IsAny<DailyChallenge>())).ReturnsAsync((DailyChallenge c) => c);
        var studentRepository = new Mock<IStudentRepository>();
        // 指定が無ければ、すべての種類がレベル5（最大難易度6）
        var student = new Student
        {
            Id = StudentId,
            Levels = (levels ?? StudentLevelRules.GradeLeveledTypes.ToDictionary(t => t, _ => 5))
                .Select(l => new StudentLevel { StudentId = StudentId, CalculationType = l.Key, LevelId = l.Value }).ToList()
        };
        studentRepository.Setup(r => r.GetByIdWithDetailsAsync(StudentId)).ReturnsAsync(student);
        var levelRepository = new Mock<ILevelRepository>();
        foreach (var level in Enumerable.Range(1, 10))
        {
            levelRepository.Setup(r => r.GetByIdAsync(level)).ReturnsAsync(new Level { Id = level, LevelNumber = level, MinDifficulty = level, MaxDifficulty = Math.Min(level + 1, 10) });
        }

        var service = new DailyChallengeService(
            challengeRepository.Object,
            problemRepository.Object,
            studentRepository.Object,
            Mock.Of<ILearningRecordRepository>(),
            levelRepository.Object,
            new FixedTimeProvider(now));
        return (service, challengeRepository);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
