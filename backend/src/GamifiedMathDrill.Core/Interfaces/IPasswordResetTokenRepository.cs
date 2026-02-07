using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

/// <summary>
/// パスワードリセットトークンのリポジトリインターフェース
/// </summary>
public interface IPasswordResetTokenRepository
{
    /// <summary>
    /// トークンハッシュで有効なトークンを取得
    /// </summary>
    /// <param name="tokenHash">ハッシュ化されたトークン値</param>
    /// <returns>有効なトークン、または null</returns>
    Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash);

    /// <summary>
    /// IDでトークンを取得
    /// </summary>
    /// <param name="id">トークンID</param>
    /// <returns>トークン、または null</returns>
    Task<PasswordResetToken?> GetByIdAsync(int id);

    /// <summary>
    /// ユーザーの有効なトークン一覧を取得
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <returns>有効なトークンのリスト</returns>
    Task<List<PasswordResetToken>> GetActiveTokensForUserAsync(string userId);

    /// <summary>
    /// トークンを作成
    /// </summary>
    /// <param name="token">作成するトークン</param>
    /// <returns>作成されたトークン</returns>
    Task<PasswordResetToken> CreateAsync(PasswordResetToken token);

    /// <summary>
    /// トークンを更新
    /// </summary>
    /// <param name="token">更新するトークン</param>
    Task UpdateAsync(PasswordResetToken token);

    /// <summary>
    /// 期限切れトークンを削除
    /// </summary>
    /// <returns>削除されたトークンの数</returns>
    Task<int> DeleteExpiredTokensAsync();
}
