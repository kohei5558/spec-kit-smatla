using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

public interface IRewardRepository : IRepository<Reward>
{
    Task<List<Reward>> GetByCategoryAsync(RewardCategory category);
    Task<List<Reward>> GetAffordableRewardsAsync(int studentPoints);
}
