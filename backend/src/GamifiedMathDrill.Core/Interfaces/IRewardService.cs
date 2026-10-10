using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

public interface IRewardService
{
    /// <summary>
    /// 家庭の景品一覧（他の家庭の景品は含まない）
    /// </summary>
    Task<IEnumerable<Reward>> GetRewardsAsync(string parentId, string? category = null, int? maxPoints = null);

    /// <summary>
    /// 新しい家庭に初期景品をコピーする
    /// </summary>
    Task CopyStarterRewardsAsync(string parentId);
    Task<Reward?> GetRewardByIdAsync(int id);
    Task<Reward> CreateRewardAsync(Reward reward, Stream? imageStream, string? fileName, string? contentType, string createdBy);
    Task<Reward> UpdateRewardAsync(int id, Reward reward, Stream? imageStream, string? fileName, string? contentType, string updatedBy);
    Task DeleteRewardAsync(int id);
    Task<(bool Success, string Message, AcquiredReward? AcquiredReward)> ExchangeRewardAsync(int studentId, int rewardId);
    Task<IEnumerable<AcquiredReward>> GetAcquiredRewardsAsync(int studentId);
}
