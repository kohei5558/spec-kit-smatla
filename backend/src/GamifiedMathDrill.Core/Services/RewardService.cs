using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Services;

public class RewardService : IRewardService
{
    private readonly IRewardRepository _rewardRepository;
    private readonly IAcquiredRewardRepository _acquiredRewardRepository;
    private readonly IStudentRepository _studentRepository;

    public RewardService(
        IRewardRepository rewardRepository,
        IAcquiredRewardRepository acquiredRewardRepository,
        IStudentRepository studentRepository)
    {
        _rewardRepository = rewardRepository;
        _acquiredRewardRepository = acquiredRewardRepository;
        _studentRepository = studentRepository;
    }

    public async Task<IEnumerable<Reward>> GetRewardsAsync(string? category = null, int? maxPoints = null)
    {
        var rewards = await _rewardRepository.GetAllAsync();

        // Filter by category if specified
        if (!string.IsNullOrWhiteSpace(category) && category != "All")
        {
            rewards = rewards.Where(r => string.Equals(r.Category.ToString(), category, StringComparison.OrdinalIgnoreCase));
        }

        // Filter by max points if specified
        if (maxPoints.HasValue)
        {
            rewards = rewards.Where(r => r.RequiredPoints <= maxPoints.Value);
        }

        return rewards.OrderBy(r => r.RequiredPoints).ThenBy(r => r.Name);
    }

    public async Task<(bool Success, string Message, AcquiredReward? AcquiredReward)> ExchangeRewardAsync(int studentId, int rewardId)
    {
        // Get student
        var student = await _studentRepository.GetByIdAsync(studentId);
        if (student == null)
        {
            return (false, "生徒が見つかりませんでした。", null);
        }

        // Get reward
        var reward = await _rewardRepository.GetByIdAsync(rewardId);
        if (reward == null)
        {
            return (false, "景品が見つかりませんでした。", null);
        }

        // Check if student has sufficient points
        if (student.TotalPoints < reward.RequiredPoints)
        {
            var shortfall = reward.RequiredPoints - student.TotalPoints;
            return (false, $"ポイントが足りません。あと{shortfall}ポイント必要です。", null);
        }

        // Perform transaction (atomic update)
        try
        {
            // Deduct points from student
            student.TotalPoints -= reward.RequiredPoints;
            await _studentRepository.UpdateAsync(student);

            // Create acquired reward record
            var acquiredReward = new AcquiredReward
            {
                StudentId = studentId,
                RewardId = rewardId,
                PointsSpent = reward.RequiredPoints,
                AcquiredAt = DateTime.UtcNow,
                Reward = reward
            };

            var savedAcquiredReward = await _acquiredRewardRepository.AddAsync(acquiredReward);

            return (true, $"「{reward.Name}」を交換しました！", savedAcquiredReward);
        }
        catch (Exception ex)
        {
            // Rollback would be handled by transaction scope in production
            throw new InvalidOperationException("景品の交換に失敗しました。", ex);
        }
    }

    public async Task<IEnumerable<AcquiredReward>> GetAcquiredRewardsAsync(int studentId)
    {
        var acquiredRewards = await _acquiredRewardRepository.GetByStudentIdAsync(studentId);
        return acquiredRewards.OrderByDescending(ar => ar.AcquiredAt);
    }
}
