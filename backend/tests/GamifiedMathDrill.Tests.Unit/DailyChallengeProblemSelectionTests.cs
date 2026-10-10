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
    public async Task Challenge_UsesLevelMaxDifficulty_AndNeverRemainderProblem()
    {
        var problems = new List<Problem>
        {
            new() { Id = 1, CalculationType = CalculationType.DivisionWithRemainder, DifficultyLevel = 6, CorrectRemainder = 1 },
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

    private static (DailyChallengeService, Mock<IDailyChallengeRepository>) CreateService(List<Problem> problems, DateTimeOffset now)
    {
        var problemRepository = new Mock<IProblemRepository>();
        problemRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(problems);
        var challengeRepository = new Mock<IDailyChallengeRepository>();
        challengeRepository.Setup(r => r.GetByStudentAndDateAsync(It.IsAny<int>(), It.IsAny<DateOnly>())).ReturnsAsync((DailyChallenge?)null);
        challengeRepository.Setup(r => r.AddOrGetExistingAsync(It.IsAny<DailyChallenge>())).ReturnsAsync((DailyChallenge c) => c);
        var studentRepository = new Mock<IStudentRepository>();
        studentRepository.Setup(r => r.GetByIdAsync(StudentId)).ReturnsAsync(new Student { Id = StudentId, CurrentLevelId = 5 });
        var levelRepository = new Mock<ILevelRepository>();
        levelRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Level { Id = 5, LevelNumber = 5, MinDifficulty = 5, MaxDifficulty = 6 });

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
