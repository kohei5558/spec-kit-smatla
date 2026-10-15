using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace GamifiedMathDrill.Infrastructure.Services;

/// <summary>
/// 景品の画像を DB に保存する（012）。URL は /api/reward-images/{id}
/// </summary>
public class DatabaseImageStorageService : IImageStorageService
{
    public const string UrlPrefix = "/api/reward-images/";

    // 無料の DB（1GB）を使い切らないよう小さめにする
    private const long MaxFileSize = 1024 * 1024;
    private static readonly HashSet<string> AllowedContentTypes = new() { "image/jpeg", "image/png", "image/gif" };

    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseImageStorageService> _logger;

    public DatabaseImageStorageService(ApplicationDbContext context, ILogger<DatabaseImageStorageService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> SaveImageAsync(Stream imageStream, string fileName, string contentType)
    {
        var type = contentType.ToLowerInvariant();
        if (!AllowedContentTypes.Contains(type))
        {
            throw new ArgumentException($"サポートされていない画像形式です: {contentType}");
        }

        if (imageStream.Length > MaxFileSize)
        {
            throw new ArgumentException($"ファイルサイズが大きすぎます（最大1MB）: {imageStream.Length} bytes");
        }

        using var buffer = new MemoryStream();
        await imageStream.CopyToAsync(buffer);
        var image = new RewardImage { Id = Guid.NewGuid(), ContentType = type, Data = buffer.ToArray(), CreatedAt = DateTime.UtcNow };
        _context.RewardImages.Add(image);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Reward image saved: {ImageId} ({Size} bytes)", image.Id, image.Data.Length);
        return $"{UrlPrefix}{image.Id:N}";
    }

    public async Task DeleteImageAsync(string? imageUrl)
    {
        // 初期景品の画像（画面側）や以前のディスク上の画像（/uploads/）は消さない
        if (!TryGetId(imageUrl, out var id))
        {
            return;
        }

        var image = await _context.RewardImages.FindAsync(id);
        if (image != null)
        {
            _context.RewardImages.Remove(image);
            await _context.SaveChangesAsync();
        }
    }

    public static bool TryGetId(string? imageUrl, out Guid id)
    {
        id = Guid.Empty;
        return imageUrl != null
            && imageUrl.StartsWith(UrlPrefix, StringComparison.OrdinalIgnoreCase)
            && Guid.TryParse(imageUrl[UrlPrefix.Length..], out id);
    }
}
