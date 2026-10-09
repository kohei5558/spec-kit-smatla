using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GamifiedMathDrill.Infrastructure.Services;

/// <summary>
/// 学習者へのアクセス可否の判定。子供ユーザーの StudentId（子供ログイン時に紐付く）を基準にする
/// </summary>
public class StudentAccessService : IStudentAccessService
{
    private readonly ApplicationDbContext _context;

    public StudentAccessService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CanAccessAsync(string userId, string role, int studentId)
    {
        if (studentId <= 0)
        {
            return false;
        }

        return role switch
        {
            nameof(UserRole.Child) => await GetOwnStudentIdAsync(userId) == studentId,
            nameof(UserRole.Parent) => await _context.Users.AnyAsync(u =>
                u.ParentId == userId && u.Role == UserRole.Child && u.StudentId == studentId),
            _ => false
        };
    }

    public async Task<int?> GetOwnStudentIdAsync(string childUserId)
    {
        return await _context.Users
            .Where(u => u.Id == childUserId && u.Role == UserRole.Child)
            .Select(u => u.StudentId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<int>> GetChildrenStudentIdsAsync(string parentId)
    {
        return await _context.Users
            .Where(u => u.ParentId == parentId && u.Role == UserRole.Child && u.StudentId != null)
            .Select(u => u.StudentId!.Value)
            .ToListAsync();
    }
}
