namespace GamifiedMathDrill.Client.Models;

/// <summary>
/// ログインレスポンス
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// 成功フラグ
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// エラーメッセージ（失敗時）
    /// </summary>
    public string? ErrorMessage { get; set; }
    
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
    /// ユーザーロール（文字列）
    /// </summary>
    public string? Role { get; set; }
    
    /// <summary>
    /// 親アカウントID（子供の場合のみ）
    /// </summary>
    public string? ParentId { get; set; }
    
    /// <summary>
    /// トークン有効期限
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}
