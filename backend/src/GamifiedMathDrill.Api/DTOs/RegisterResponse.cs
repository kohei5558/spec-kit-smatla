namespace GamifiedMathDrill.Api.DTOs;

/// <summary>
/// 新規アカウント作成レスポンス
/// </summary>
public class RegisterResponse
{
    /// <summary>
    /// 登録成功フラグ
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 成功またはエラーメッセージ
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 作成されたユーザーのID（成功時のみ）
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// JWTトークン（自動ログイン用、成功時のみ）
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// 表示名（成功時のみ）
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// メールアドレス（成功時のみ）
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// ユーザーロール（成功時のみ）
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// トークン有効期限（成功時のみ）
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}
