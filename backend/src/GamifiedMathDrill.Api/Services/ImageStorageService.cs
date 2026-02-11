using GamifiedMathDrill.Core.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace GamifiedMathDrill.Api.Services;

/// <summary>
/// 画像ストレージサービス（ローカルファイルシステム）
/// </summary>
public class ImageStorageService : IImageStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ImageStorageService> _logger;
    private const string UploadFolder = "uploads/rewards";
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
    private static readonly HashSet<string> AllowedContentTypes = new()
    {
        "image/jpeg",
        "image/png",
        "image/gif"
    };

    public ImageStorageService(IWebHostEnvironment environment, ILogger<ImageStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> SaveImageAsync(Stream imageStream, string fileName, string contentType)
    {
        try
        {
            // コンテンツタイプ検証
            if (!AllowedContentTypes.Contains(contentType.ToLowerInvariant()))
            {
                throw new ArgumentException($"サポートされていない画像形式です: {contentType}");
            }

            // ファイルサイズ検証
            if (imageStream.Length > MaxFileSize)
            {
                throw new ArgumentException($"ファイルサイズが大きすぎます（最大5MB）: {imageStream.Length} bytes");
            }

            // WebRootPath が設定されていない場合は ContentRootPath/wwwroot をフォールバック
            var webRoot = _environment.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(_environment.ContentRootPath ?? Directory.GetCurrentDirectory(), "wwwroot");
            }

            // アップロードディレクトリ作成
            var uploadPath = Path.Combine(webRoot, UploadFolder);
            Directory.CreateDirectory(uploadPath);

            // ユニークなファイル名生成
            var extension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadPath, uniqueFileName);

            // ファイル保存
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageStream.CopyToAsync(fileStream);
            }

            // 相対パスを返す
            var relativeUrl = $"/{UploadFolder}/{uniqueFileName}";
            _logger.LogInformation("画像を保存しました: {Url}", relativeUrl);
            return relativeUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "画像保存中にエラーが発生しました: {FileName}", fileName);
            throw;
        }
    }

    public Task DeleteImageAsync(string? imageUrl)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return Task.CompletedTask;
            }

            // URLから物理パスを構築
            var fileName = Path.GetFileName(imageUrl);
            var webRootForDelete = _environment?.WebRootPath;
            if (string.IsNullOrEmpty(webRootForDelete))
            {
                webRootForDelete = Path.Combine(_environment?.ContentRootPath ?? Directory.GetCurrentDirectory(), "wwwroot");
            }

            if (string.IsNullOrEmpty(webRootForDelete))
            {
                // 最終フォールバック
                webRootForDelete = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            // 詳細ログ: 組み立てるパスの中身を記録（デバッグ用）
            _logger.LogDebug("Deleting image - webRoot: {WebRoot}, uploadFolder: {UploadFolder}, fileName: {FileName}", webRootForDelete, UploadFolder, fileName);

            var filePath = Path.Combine(webRootForDelete, UploadFolder, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("画像を削除しました: {Url}", imageUrl);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "画像削除中にエラーが発生しました: {Url}", imageUrl);
            throw;
        }
    }
}
