using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

public interface IRewardService
{
    Task<IEnumerable<Reward>> GetRewardsAsync(string? category = null, int? maxPoints = null);
    Task<(bool Success, string Message, AcquiredReward? AcquiredReward)> ExchangeRewardAsync(int studentId, int rewardId);
    Task<IEnumerable<AcquiredReward>> GetAcquiredRewardsAsync(int studentId);
}
