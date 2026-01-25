using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

public interface IRewardService
{
    Task<IEnumerable<Reward>> GetRewardsAsync(string? category = null, int? maxPoints = null);
    Task<Reward?> GetRewardByIdAsync(int id);
    Task<Reward> CreateRewardAsync(Reward reward, Stream? imageStream, string? fileName, string? contentType, string createdBy);
    Task<Reward> UpdateRewardAsync(int id, Reward reward, Stream? imageStream, string? fileName, string? contentType, string updatedBy);
    Task DeleteRewardAsync(int id);
    Task<(bool Success, string Message, AcquiredReward? AcquiredReward)> ExchangeRewardAsync(int studentId, int rewardId);
    Task<IEnumerable<AcquiredReward>> GetAcquiredRewardsAsync(int studentId);
}
