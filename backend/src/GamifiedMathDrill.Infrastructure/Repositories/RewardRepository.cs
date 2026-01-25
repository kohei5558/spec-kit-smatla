using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GamifiedMathDrill.Infrastructure.Repositories;

public class RewardRepository : Repository<Reward>, IRewardRepository
{
    public RewardRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<Reward> UpdateAsync(Reward entity)
    {
        try
        {
            return await base.UpdateAsync(entity);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("他のユーザーによって景品が更新されました。最新のデータを取得してやり直してください。");
        }
    }

    public async Task<List<Reward>> GetByCategoryAsync(RewardCategory category)
    {
        return await _context.Set<Reward>()
            .Where(r => r.Category == category)
            .OrderBy(r => r.RequiredPoints)
            .ToListAsync();
    }

    public async Task<List<Reward>> GetAffordableRewardsAsync(int studentPoints)
    {
        return await _context.Set<Reward>()
            .Where(r => r.RequiredPoints <= studentPoints)
            .OrderBy(r => r.RequiredPoints)
            .ToListAsync();
    }
}
