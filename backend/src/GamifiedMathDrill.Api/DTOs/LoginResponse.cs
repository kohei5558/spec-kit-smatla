using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Api.DTOs;

/// <summary>
/// ログインレスポンス
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// JWTトークン
    /// </summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// ユーザーID
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// 表示名
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// ユーザーロール
    /// </summary>
    public UserRole Role { get; set; }
    
    /// <summary>
    /// 親アカウントID（子供の場合のみ）
    /// </summary>
    public string? ParentId { get; set; }
    
    /// <summary>
    /// トークン有効期限
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
