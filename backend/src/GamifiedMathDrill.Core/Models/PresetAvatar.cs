namespace GamifiedMathDrill.Core.Models;

/// <summary>
/// プリセットアバター
/// 子供アカウント作成時に選択可能なアバター画像
/// </summary>
public class PresetAvatar
{
    /// <summary>
    /// アバターID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// アバター名（例：「猫」「犬」「パンダ」）
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// ファイル名（例：「cat.png」）
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 表示順序
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
