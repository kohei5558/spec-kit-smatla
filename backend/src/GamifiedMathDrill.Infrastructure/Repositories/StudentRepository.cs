using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GamifiedMathDrill.Infrastructure.Repositories;

/// <summary>
/// 児童リポジトリ実装
/// </summary>
public class StudentRepository : Repository<Student>, IStudentRepository
{
    public StudentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Student?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(s => s.CurrentLevel)
            .Include(s => s.LearningRecords)
            .Include(s => s.AcquiredRewards)
            .FirstOrDefaultAsync(s => s.Id == id);
    }
}
