namespace GamifiedMathDrill.Client.Models;

/// <summary>
/// 子供ログインリクエスト
/// </summary>
public class ChildLoginRequest
{
    /// <summary>
    /// 子供のアカウントID
    /// </summary>
    public string ChildAccountId { get; set; } = string.Empty;
    
    /// <summary>
    /// PIN（4桁）
    /// </summary>
    public string PIN { get; set; } = string.Empty;
}
