using Xunit;
using Moq;
using GamifiedMathDrill.Core.Services;
using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Tests.Unit;

/// <summary>
/// RewardService の基本的な動作確認テスト
/// </summary>
public class RewardServiceBasicTests
{
    private readonly Mock<IRewardRepository> _mockRewardRepository;
    private readonly Mock<IAcquiredRewardRepository> _mockAcquiredRewardRepository;
    private readonly Mock<IStudentRepository> _mockStudentRepository;
    private readonly Mock<IImageStorageService> _mockImageStorageService;
    private readonly RewardService _rewardService;

    public RewardServiceBasicTests()
    {
        _mockRewardRepository = new Mock<IRewardRepository>();
        _mockAcquiredRewardRepository = new Mock<IAcquiredRewardRepository>();
        _mockStudentRepository = new Mock<IStudentRepository>();
        _mockImageStorageService = new Mock<IImageStorageService>();

        _rewardService = new RewardService(
            _mockRewardRepository.Object,
            _mockAcquiredRewardRepository.Object,
            _mockStudentRepository.Object,
            _mockImageStorageService.Object
        );
    }

    [Fact]
    public async Task GetRewardsAsync_ReturnsRewards()
    {
        // Arrange
        var rewards = new List<Reward>
        {
            new Reward { Id = 1, Name = "Test Snack", RequiredPoints = 100, Category = RewardCategory.Snack },
            new Reward { Id = 2, Name = "Test Card", RequiredPoints = 200, Category = RewardCategory.Card }
        };
        _mockRewardRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(rewards);

        // Act
        var result = await _rewardService.GetRewardsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task ExchangeRewardAsync_WithInsufficientPoints_ReturnsFailure()
    {
        // Arrange
        var student = new Student
        {
            Id = 1,
            Name = "Test Student",
            TotalPoints = 10
        };
        var reward = new Reward { Id = 1, Name = "Snack", RequiredPoints = 100, Category = RewardCategory.Snack };

        _mockStudentRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(student);
        _mockRewardRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(reward);

        // Act
        var result = await _rewardService.ExchangeRewardAsync(1, 1);

        // Assert
        Assert.False(result.Success);
    }
}
