using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GamifiedMathDrill.Infrastructure.Repositories;

/// <summary>
/// 学習記録リポジトリ実装
/// </summary>
public class LearningRecordRepository : Repository<LearningRecord>, ILearningRecordRepository
{
    public LearningRecordRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<LearningRecord>> GetByStudentIdAsync(int studentId)
    {
        return await _dbSet
            .Where(lr => lr.StudentId == studentId)
            .Include(lr => lr.Problem)
            .OrderByDescending(lr => lr.SolvedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<LearningRecord>> GetByStudentIdAndDateRangeAsync(int studentId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(lr => lr.StudentId == studentId && lr.SolvedAt >= startDate && lr.SolvedAt <= endDate)
            .Include(lr => lr.Problem)
            .OrderByDescending(lr => lr.SolvedAt)
            .ToListAsync();
    }
}
