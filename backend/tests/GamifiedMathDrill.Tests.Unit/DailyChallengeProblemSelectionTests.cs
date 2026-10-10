using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Core.Services;
using Moq;
using Xunit;

namespace GamifiedMathDrill.Tests.Unit;

/// <summary>
/// デイリーチャレンジ（答えは1つ）に、あまりのあるわり算を選ばないことのテスト（009）
/// </summary>
public class DailyChallengeProblemSelectionTests
{
    [Fact]
    public async Task CreateDailyChallenge_NeverSelectsRemainderProblem()
    {
        var problems = new List<Problem>
        {
            new() { Id = 1, CalculationType = CalculationType.DivisionWithRemainder, DifficultyLevel = 7, CorrectRemainder = 1 },
            new() { Id = 2, CalculationType = CalculationType.DivisionWithRemainder, DifficultyLevel = 8, CorrectRemainder = 2 },
            new() { Id = 3, CalculationType = CalculationType.Division, DifficultyLevel = 7 }
        };
        var problemRepository = new Mock<IProblemRepository>();
        problemRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(problems);
        var challengeRepository = new Mock<IDailyChallengeRepository>();
        challengeRepository.Setup(r => r.GetByDateAsync(It.IsAny<DateOnly>())).ReturnsAsync((DailyChallenge?)null);
        challengeRepository.Setup(r => r.AddAsync(It.IsAny<DailyChallenge>())).ReturnsAsync((DailyChallenge c) => c);
        var service = new DailyChallengeService(
            challengeRepository.Object,
            problemRepository.Object,
            Mock.Of<IStudentRepository>(),
            Mock.Of<ILearningRecordRepository>(),
            Mock.Of<ILevelRepository>());

        // 乱数で選ばれるため、何日分か作ってすべて割り切れる問題であることを確認する
        for (var day = 0; day < 20; day++)
        {
            var challenge = await service.CreateDailyChallengeAsync(new DateTime(2026, 10, 1).AddDays(day));
            Assert.Equal(3, challenge.ProblemId);
        }
    }
}
