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
