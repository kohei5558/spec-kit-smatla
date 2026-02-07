namespace GamifiedMathDrill.Client.Models;

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
    /// メッセージ（成功またはエラーメッセージ）
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// ユーザーID
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// JWTトークン（自動ログイン用）
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// 表示名
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// メールアドレス
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// ユーザーロール
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// トークン有効期限
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}
