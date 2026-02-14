using System.Text.Json.Serialization;

namespace GamifiedMathDrill.Client.Models;

/// <summary>
/// 子供ログインリクエスト
/// </summary>
public class ChildLoginRequest
{
    /// <summary>
    /// 子供のアカウントID
    /// Backend expects JSON property `ChildId` so map accordingly.
    /// </summary>
    [JsonPropertyName("ChildId")]
    public string ChildAccountId { get; set; } = string.Empty;
    
    /// <summary>
    /// PIN（4桁）
    /// </summary>
    public string PIN { get; set; } = string.Empty;
}
