namespace GamifiedMathDrill.Client.Services;

/// <summary>
/// 画像の URL を画面で表示できる形にする。
/// 保護者がアップロードした景品の画像（/uploads/...）は API サーバー側にあるため、API のアドレスを付ける。
/// 初期景品・アバターの画像（/images/..., /avatars/...）はこの画面のサーバー（wwwroot）にあるのでそのまま
/// </summary>
public static class ImageUrls
{
    public const string ApiBaseAddress = "http://localhost:5242";

    public static string? Resolve(string? url) =>
        url != null && url.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase)
            ? ApiBaseAddress + url
            : url;
}
