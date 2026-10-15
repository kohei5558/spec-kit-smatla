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

    public async Task<DailyChallenge?> GetByStudentAndDateAsync(int studentId, DateOnly date)
    {
        return await _dbSet.FirstOrDefaultAsync(dc => dc.StudentId == studentId && dc.TargetDate == date);
    }

    public async Task<DailyChallenge> AddOrGetExistingAsync(DailyChallenge challenge)
    {
        try
        {
            return await AddAsync(challenge);
        }
        catch (DbUpdateException)
        {
            // 同じ学習者・日のチャレンジが同時に作られた（一意制約）。先に作られた方を使う
            _context.Entry(challenge).State = EntityState.Detached;
            var existing = await GetByStudentAndDateAsync(challenge.StudentId, challenge.TargetDate);
            return existing ?? throw new InvalidOperationException("Daily challenge could not be created.");
        }
    }

    public async Task<bool> TrySaveAnswerAsync(DailyChallenge challenge)
    {
        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            // AnsweredAt（同時実行の確認用）が他の回答で先に変わっていた
            _context.Entry(challenge).State = EntityState.Detached;
            return false;
        }
    }
}
