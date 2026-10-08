using System.Text.Json.Serialization;

namespace GamifiedMathDrill.Api.DTOs;

/// <summary>
/// 子供ログインリクエスト
/// </summary>
public class ChildLoginRequest
{
    /// <summary>
    /// 子供アカウントID（JSON上のキーは "ChildId"。フロントエンドの ChildLoginRequest と一致させる）
    /// </summary>
    [JsonPropertyName("ChildId")]
    public string ChildAccountId { get; set; } = string.Empty;

    /// <summary>
    /// PIN（4桁）
    /// </summary>
    public string PIN { get; set; } = string.Empty;
}
