using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GamifiedMathDrill.Infrastructure.Repositories;

/// <summary>
/// 問題リポジトリ実装
/// </summary>
public class ProblemRepository : Repository<Problem>, IProblemRepository
{
    public ProblemRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Problem?> GetRandomProblemAsync(int difficultyLevel, CalculationType? type = null, List<int>? excludeIds = null)
    {
        // 使わない問題（最新の問題セットにない、011）は出題しない
        var query = _dbSet.Where(p => p.IsActive && p.DifficultyLevel == difficultyLevel);

        if (type.HasValue)
        {
            query = query.Where(p => p.CalculationType == type.Value);
        }

        if (excludeIds != null && excludeIds.Any())
        {
            query = query.Where(p => !excludeIds.Contains(p.Id));
        }

        var problems = await query.ToListAsync();
        if (!problems.Any())
        {
            return null;
        }

        var random = new Random();
        return problems[random.Next(problems.Count)];
    }

    public async Task<IEnumerable<Problem>> GetProblemsByDifficultyAsync(int minDifficulty, int maxDifficulty)
    {
        return await _dbSet
            .Where(p => p.IsActive && p.DifficultyLevel >= minDifficulty && p.DifficultyLevel <= maxDifficulty)
            .ToListAsync();
    }
}
