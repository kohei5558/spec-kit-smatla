using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GamifiedMathDrill.Infrastructure.Repositories;

/// <summary>
/// パスワードリセットトークンのリポジトリ実装
/// </summary>
public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly ApplicationDbContext _context;

    public PasswordResetTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash)
    {
        return await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => 
                t.TokenHash == tokenHash && 
                !t.IsUsed && 
                t.ExpiresAt > DateTime.UtcNow);
    }

    /// <inheritdoc/>
    public async Task<PasswordResetToken?> GetByIdAsync(int id)
    {
        return await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    /// <inheritdoc/>
    public async Task<List<PasswordResetToken>> GetActiveTokensForUserAsync(string userId)
    {
        return await _context.PasswordResetTokens
            .Where(t => 
                t.UserId == userId && 
                !t.IsUsed && 
                t.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<PasswordResetToken> CreateAsync(PasswordResetToken token)
    {
        _context.PasswordResetTokens.Add(token);
        await _context.SaveChangesAsync();
        return token;
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PasswordResetToken token)
    {
        _context.PasswordResetTokens.Update(token);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<int> DeleteExpiredTokensAsync()
    {
        var expiredTokens = await _context.PasswordResetTokens
            .Where(t => t.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();
        
        _context.PasswordResetTokens.RemoveRange(expiredTokens);
        return await _context.SaveChangesAsync();
    }
}
