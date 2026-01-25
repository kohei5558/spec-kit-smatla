using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

/// <summary>
/// 認証サービスのインターフェース
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// 保護者ログイン
    /// </summary>
    /// <param name="email">メールアドレス</param>
    /// <param name="password">パスワード</param>
    /// <returns>ログインレスポンスデータ(userId, displayName, role, parentId, token, expiresAt)</returns>
    Task<(string? userId, string? displayName, UserRole? role, string? parentId, string? token, DateTime? expiresAt)> LoginAsync(string email, string password);
    
    /// <summary>
    /// 子供ログイン
    /// </summary>
    /// <param name="childId">子供のユーザーID</param>
    /// <param name="pin">PIN（4桁）</param>
    /// <returns>ログインレスポンスデータ(userId, displayName, role, parentId, token, expiresAt)</returns>
    Task<(string? userId, string? displayName, UserRole? role, string? parentId, string? token, DateTime? expiresAt)> ChildLoginAsync(string childId, string pin);
    
    /// <summary>
    /// JWTトークンを生成
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="displayName">表示名</param>
    /// <param name="role">ロール</param>
    /// <param name="parentId">親ID（子供の場合）</param>
    /// <returns>JWTトークン</returns>
    string GenerateJwtToken(string userId, string displayName, string role, string? parentId = null);
}
