using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GamifiedMathDrill.Infrastructure.Repositories;

public class DailyChallengeRepository : Repository<DailyChallenge>, IDailyChallengeRepository
{
    public DailyChallengeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<DailyChallenge?> GetByDateAsync(DateOnly date)
    {
        return await _context.Set<DailyChallenge>()
            .Include(dc => dc.Problem)
            .FirstOrDefaultAsync(dc => dc.TargetDate == date);
    }
}
