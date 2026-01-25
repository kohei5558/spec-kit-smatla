namespace GamifiedMathDrill.Api.DTOs;

/// <summary>
/// 子供ログインリクエスト
/// </summary>
public class ChildLoginRequest
{
    /// <summary>
    /// 子供のユーザーID
    /// </summary>
    public string ChildId { get; set; } = string.Empty;
    
    /// <summary>
    /// PIN（4桁）
    /// </summary>
    public string PIN { get; set; } = string.Empty;
}
