using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GamifiedMathDrill.Infrastructure.Repositories;

/// <summary>
/// 交換申請リポジトリ
/// </summary>
public class ExchangeRequestRepository : IExchangeRequestRepository
{
    private readonly ApplicationDbContext _context;

    public ExchangeRequestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ExchangeRequest>> GetAllAsync()
    {
        return await _context.ExchangeRequests
            .Include(er => er.Student)
            .Include(er => er.Reward)
            .OrderByDescending(er => er.RequestedAt)
            .ToListAsync();
    }

    public async Task<ExchangeRequest?> GetByIdAsync(int id)
    {
        return await _context.ExchangeRequests
            .Include(er => er.Student)
            .Include(er => er.Reward)
            .FirstOrDefaultAsync(er => er.Id == id);
    }

    public async Task<ExchangeRequest> AddAsync(ExchangeRequest entity)
    {
        await _context.ExchangeRequests.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<ExchangeRequest> UpdateAsync(ExchangeRequest entity)
    {
        _context.ExchangeRequests.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(ExchangeRequest entity)
    {
        _context.ExchangeRequests.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.ExchangeRequests.AnyAsync(er => er.Id == id);
    }

    public async Task<IEnumerable<ExchangeRequest>> GetByStudentIdAsync(int studentId)
    {
        return await _context.ExchangeRequests
            .Include(er => er.Student)
            .Include(er => er.Reward)
            .Where(er => er.StudentId == studentId)
            .OrderByDescending(er => er.RequestedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExchangeRequest>> GetByStatusAsync(ExchangeStatus status)
    {
        return await _context.ExchangeRequests
            .Include(er => er.Student)
            .Include(er => er.Reward)
            .Where(er => er.Status == status)
            .OrderByDescending(er => er.RequestedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExchangeRequest>> GetByParentIdAsync(string parentId)
    {
        // Student テーブルに ParentId を追加する必要がある場合
        // 今は ApplicationUser.ParentId を使う想定
        return await _context.ExchangeRequests
            .Include(er => er.Student)
            .Include(er => er.Reward)
            .OrderByDescending(er => er.RequestedAt)
            .ToListAsync();
    }
}
