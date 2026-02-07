namespace GamifiedMathDrill.Core.DTOs;

/// <summary>
/// プリセットアバター情報（レスポンス）
/// </summary>
public class PresetAvatarDto
{
    /// <summary>
    /// アバターID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// アバター名（例：「猫」「犬」）
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// アバター画像URL（例：「/avatars/cat.png」）
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// 表示順序
    /// </summary>
    public int DisplayOrder { get; set; }
}
