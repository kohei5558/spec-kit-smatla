using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

/// <summary>
/// プリセットアバターリポジトリ
/// </summary>
public interface IPresetAvatarRepository
{
    /// <summary>
    /// 全プリセットアバターを取得（表示順でソート）
    /// </summary>
    Task<List<PresetAvatar>> GetAllAsync();

    /// <summary>
    /// IDでプリセットアバターを取得
    /// </summary>
    Task<PresetAvatar?> GetByIdAsync(int id);
}
