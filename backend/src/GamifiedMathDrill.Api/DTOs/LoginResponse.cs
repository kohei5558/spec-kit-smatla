using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Api.DTOs;

/// <summary>
/// ログインレスポンス
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// ログイン成功フラグ
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// JWTトークン
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// ユーザーID
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// 表示名
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// ユーザーロール
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// 親アカウントID（子供の場合のみ）
    /// </summary>
    public string? ParentId { get; set; }

    /// <summary>
    /// 学生ID（子供アカウントの場合のみ）
    /// </summary>
    public int? StudentId { get; set; }

    /// <summary>
    /// トークン有効期限
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// エラーメッセージ（失敗時のみ）
    /// </summary>
    public string? ErrorMessage { get; set; }
}
