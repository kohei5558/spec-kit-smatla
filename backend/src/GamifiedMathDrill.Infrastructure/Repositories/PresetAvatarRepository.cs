using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GamifiedMathDrill.Infrastructure.Repositories;

/// <summary>
/// プリセットアバターリポジトリ実装
/// </summary>
public class PresetAvatarRepository : IPresetAvatarRepository
{
    private readonly ApplicationDbContext _context;

    public PresetAvatarRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 全プリセットアバターを取得（表示順でソート）
    /// </summary>
    public async Task<List<PresetAvatar>> GetAllAsync()
    {
        return await _context.PresetAvatars
            .OrderBy(a => a.DisplayOrder)
            .ToListAsync();
    }

    /// <summary>
    /// IDでプリセットアバターを取得
    /// </summary>
    public async Task<PresetAvatar?> GetByIdAsync(int id)
    {
        return await _context.PresetAvatars
            .FirstOrDefaultAsync(a => a.Id == id);
    }
}
