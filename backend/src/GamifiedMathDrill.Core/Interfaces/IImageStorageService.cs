namespace GamifiedMathDrill.Core.Interfaces;

/// <summary>
/// 画像ストレージサービスのインターフェース
/// </summary>
public interface IImageStorageService
{
    /// <summary>
    /// 画像を保存
    /// </summary>
    /// <param name="imageStream">画像ストリーム</param>
    /// <param name="fileName">ファイル名</param>
    /// <param name="contentType">コンテンツタイプ</param>
    /// <returns>保存された画像のURL</returns>
    Task<string> SaveImageAsync(Stream imageStream, string fileName, string contentType);

    /// <summary>
    /// 画像を削除
    /// </summary>
    /// <param name="imageUrl">画像URL</param>
    Task DeleteImageAsync(string? imageUrl);
}
