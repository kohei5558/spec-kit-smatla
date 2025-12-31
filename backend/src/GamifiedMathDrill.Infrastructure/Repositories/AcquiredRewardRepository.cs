using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GamifiedMathDrill.Infrastructure.Repositories;

public class AcquiredRewardRepository : Repository<AcquiredReward>, IAcquiredRewardRepository
{
    public AcquiredRewardRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<AcquiredReward>> GetByStudentIdAsync(int studentId)
    {
        return await _context.Set<AcquiredReward>()
            .Include(ar => ar.Reward)
            .Where(ar => ar.StudentId == studentId)
            .OrderByDescending(ar => ar.AcquiredAt)
            .ToListAsync();
    }
}
