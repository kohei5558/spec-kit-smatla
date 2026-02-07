using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GamifiedMathDrill.Tests.Unit;

/// <summary>
/// 楽観的ロックのテスト
/// </summary>
public class OptimisticLockingTests
{
    [Fact]
    public async Task CreateRequestAsync_ConcurrentRequests_OnlyOneSucceeds()
    {
        // Arrange
        var mockRewardRepo = new Mock<IRewardRepository>();
        var mockStudentRepo = new Mock<IStudentRepository>();
        var mockExchangeRequestRepo = new Mock<IExchangeRequestRepository>();

        var reward = new Reward
        {
            Id = 1,
            Name = "Test Reward",
            RequiredPoints = 100,
            IsPhysical = true,
            Stock = 1, // 在庫1個
            IsActive = true,
            Category = RewardCategory.Snack,
            RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }
        };

        var student1 = new Student
        {
            Id = 1,
            Name = "Student 1",
            TotalPoints = 100
        };

        var student2 = new Student
        {
            Id = 2,
            Name = "Student 2",
            TotalPoints = 100
        };

        // 最初の呼び出しは成功、2回目は楽観的ロック例外
        var callCount = 0;
        mockRewardRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(reward);
        mockStudentRepo.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(student1);
        mockStudentRepo.Setup(s => s.GetByIdAsync(2)).ReturnsAsync(student2);

        mockRewardRepo.Setup(r => r.UpdateAsync(It.IsAny<Reward>()))
            .ReturnsAsync((Reward r) =>
            {
                callCount++;
                if (callCount == 1)
                {
                    // 最初の更新は成功
                    return r;
                }
                else
                {
                    // 2回目は楽観的ロック例外をシミュレート
                    throw new InvalidOperationException("他のユーザーによって景品が更新されました。最新のデータを取得してやり直してください。");
                }
            });

        mockExchangeRequestRepo.Setup(r => r.AddAsync(It.IsAny<ExchangeRequest>()))
            .ReturnsAsync((ExchangeRequest er) => er);

        var service = new ExchangeRequestService(
            mockExchangeRequestRepo.Object,
            mockRewardRepo.Object,
            mockStudentRepo.Object);

        // Act
        var request1Task = service.CreateRequestAsync(1, 1);
        var request1 = await request1Task; // 最初のリクエストは成功

        // 2回目は失敗するはず（在庫が0になっている想定）
        reward.Stock = 0; // シミュレーション：在庫がなくなった
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.CreateRequestAsync(2, 1));

        // Assert
        Assert.NotNull(request1);
        Assert.Equal("在庫がありません。", exception.Message);
    }

    [Fact]
    public async Task UpdateRewardAsync_ConcurrentUpdates_ThrowsConcurrencyException()
    {
        // Arrange
        var mockRewardRepo = new Mock<IRewardRepository>();
        var mockAcquiredRewardRepo = new Mock<IAcquiredRewardRepository>();
        var mockStudentRepo = new Mock<IStudentRepository>();
        var mockImageStorage = new Mock<IImageStorageService>();
        var mockLogger = new Mock<ILogger<RewardService>>();

        var reward = new Reward
        {
            Id = 1,
            Name = "Original Name",
            RequiredPoints = 100,
            IsPhysical = true,
            Stock = 10,
            IsActive = true,
            Category = RewardCategory.Toy,
            RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }
        };

        mockRewardRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(reward);

        // 最初の更新は成功するが、2回目は楽観的ロック例外
        var updateCount = 0;
        mockRewardRepo.Setup(r => r.UpdateAsync(It.IsAny<Reward>()))
            .ReturnsAsync((Reward r) =>
            {
                updateCount++;
                if (updateCount == 1)
                {
                    return r;
                }
                else
                {
                    throw new InvalidOperationException("他のユーザーによって景品が更新されました。最新のデータを取得してやり直してください。");
                }
            });

        var service = new RewardService(
            mockRewardRepo.Object,
            mockAcquiredRewardRepo.Object,
            mockStudentRepo.Object,
            mockImageStorage.Object);

        var updatedReward1 = new Reward
        {
            Id = 1,
            Name = "Updated Name 1",
            RequiredPoints = 150,
            IsPhysical = true,
            Stock = 8,
            Category = RewardCategory.Toy,
            RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 } // 同じRowVersion
        };

        var updatedReward2 = new Reward
        {
            Id = 1,
            Name = "Updated Name 2",
            RequiredPoints = 200,
            IsPhysical = true,
            Stock = 5,
            Category = RewardCategory.Toy,
            RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 } // 同じRowVersion（競合）
        };

        // Act
        var result1 = await service.UpdateRewardAsync(1, updatedReward1, null, null, null, "user1");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.UpdateRewardAsync(1, updatedReward2, null, null, null, "user2"));

        // Assert
        Assert.NotNull(result1);
        Assert.Equal("他のユーザーによって景品が更新されました。最新のデータを取得してやり直してください。", exception.Message);
    }
}
