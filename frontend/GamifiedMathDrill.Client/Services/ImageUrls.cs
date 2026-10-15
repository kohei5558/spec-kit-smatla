namespace GamifiedMathDrill.Client.Services;

/// <summary>
/// 画像の URL を画面で表示できる形にする。
/// 保護者がアップロードした景品の画像（/api/reward-images/...、以前の /uploads/...）は API サーバー側にあるため、API のアドレスを付ける。
/// 初期景品・アバターの画像（/images/..., /avatars/...）はこの画面のサーバー（wwwroot）にあるのでそのまま
/// </summary>
public static class ImageUrls
{
    /// <summary>
    /// API のアドレス。wwwroot/appsettings.json の ApiBaseAddress（本番はデプロイ時に書き換える、012）
    /// </summary>
    public static string ApiBaseAddress { get; private set; } = "http://localhost:5242";

    public static void Configure(string? apiBaseAddress)
    {
        if (!string.IsNullOrWhiteSpace(apiBaseAddress))
        {
            ApiBaseAddress = apiBaseAddress.TrimEnd('/');
        }
    }

    public static string? Resolve(string? url) =>
        url != null && (url.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) || url.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            ? ApiBaseAddress + url
            : url;
}
