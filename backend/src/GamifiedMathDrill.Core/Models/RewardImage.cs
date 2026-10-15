namespace GamifiedMathDrill.Core.Models;

/// <summary>
/// 保護者がアップロードした景品の画像。無料のサーバーはディスクが消えるため DB に保存する（012）
/// </summary>
public class RewardImage
{
    /// <summary>
    /// ID。画像は &lt;img&gt; からログインなしで取得するため、推測できないランダムな値にする
    /// </summary>
    public Guid Id { get; set; }

    public string ContentType { get; set; } = string.Empty;

    public byte[] Data { get; set; } = Array.Empty<byte>();

    public DateTime CreatedAt { get; set; }
}
