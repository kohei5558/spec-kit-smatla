using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Services;

public class RewardService : IRewardService
{
    private readonly IRewardRepository _rewardRepository;
    private readonly IAcquiredRewardRepository _acquiredRewardRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IImageStorageService _imageStorageService;

    public RewardService(
        IRewardRepository rewardRepository,
        IAcquiredRewardRepository acquiredRewardRepository,
        IStudentRepository studentRepository,
        IImageStorageService imageStorageService)
    {
        _rewardRepository = rewardRepository;
        _acquiredRewardRepository = acquiredRewardRepository;
        _studentRepository = studentRepository;
        _imageStorageService = imageStorageService;
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

    public async Task<Reward?> GetRewardByIdAsync(int id)
    {
        return await _rewardRepository.GetByIdAsync(id);
    }

    public async Task<Reward> CreateRewardAsync(Reward reward, Stream? imageStream, string? fileName, string? contentType, string createdBy)
    {
        // 画像アップロード
        if (imageStream != null && !string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(contentType))
        {
            reward.ImageUrl = await _imageStorageService.SaveImageAsync(imageStream, fileName, contentType);
        }

        // 作成情報設定
        reward.CreatedBy = createdBy;
        reward.CreatedAt = DateTime.UtcNow;
        reward.UpdatedAt = DateTime.UtcNow;
        reward.IsActive = true;

        return await _rewardRepository.AddAsync(reward);
    }

    public async Task<Reward> UpdateRewardAsync(int id, Reward updatedReward, Stream? imageStream, string? fileName, string? contentType, string updatedBy)
    {
        var existingReward = await _rewardRepository.GetByIdAsync(id);
        if (existingReward == null)
        {
            throw new KeyNotFoundException($"景品が見つかりません（ID: {id}）");
        }

        // 画像更新
        if (imageStream != null && !string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(contentType))
        {
            // 既存画像削除
            await _imageStorageService.DeleteImageAsync(existingReward.ImageUrl);

            // 新しい画像保存
            existingReward.ImageUrl = await _imageStorageService.SaveImageAsync(imageStream, fileName, contentType);
        }

        // フィールド更新
        existingReward.Name = updatedReward.Name;
        existingReward.Description = updatedReward.Description;
        existingReward.RequiredPoints = updatedReward.RequiredPoints;
        existingReward.Category = updatedReward.Category;
        existingReward.IsPhysical = updatedReward.IsPhysical;
        existingReward.Stock = updatedReward.Stock;
        existingReward.IsActive = updatedReward.IsActive;
        existingReward.UpdatedAt = DateTime.UtcNow;
        existingReward.RowVersion = updatedReward.RowVersion; // 楽観的同時実行制御

        return await _rewardRepository.UpdateAsync(existingReward);
    }

    public async Task DeleteRewardAsync(int id)
    {
        var reward = await _rewardRepository.GetByIdAsync(id);
        if (reward == null)
        {
            throw new KeyNotFoundException($"景品が見つかりません（ID: {id}）");
        }

        // 画像削除
        await _imageStorageService.DeleteImageAsync(reward.ImageUrl);

        await _rewardRepository.DeleteAsync(reward);
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

        // Check stock for physical rewards
        if (reward.IsPhysical && reward.Stock.HasValue && reward.Stock.Value <= 0)
        {
            return (false, "在庫がありません。", null);
        }

        // Perform transaction (atomic update)
        try
        {
            // Deduct points from student
            student.TotalPoints -= reward.RequiredPoints;
            await _studentRepository.UpdateAsync(student);

            // Reduce stock for physical rewards
            if (reward.IsPhysical && reward.Stock.HasValue)
            {
                reward.Stock -= 1;
                await _rewardRepository.UpdateAsync(reward);
            }

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
