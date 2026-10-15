namespace GamifiedMathDrill.Client.Models;

/// <summary>
/// 端末登録リクエスト
/// </summary>
public class RegisterDeviceRequest
{
    /// <summary>
    /// 端末名（1〜30文字）
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// 端末登録レスポンス
/// </summary>
public class RegisterDeviceResponse
{
    /// <summary>
    /// 登録した端末
    /// </summary>
    public RegisteredDeviceViewModel Device { get; set; } = new();

    /// <summary>
    /// 端末トークン（この端末の LocalStorage にだけ保存する）
    /// </summary>
    public string Token { get; set; } = string.Empty;
}

/// <summary>
/// 登録済み端末
/// </summary>
public class RegisteredDeviceViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastUsedAt { get; set; }
}

/// <summary>
/// 子供ログイン画面用の子供一覧取得結果
/// </summary>
/// <param name="IsDeviceRegistered">この端末が子供用に登録されているか</param>
/// <param name="Children">その家庭の利用中の子供（未登録・取得失敗時は空）</param>
/// <param name="ErrorMessage">通信エラー時のメッセージ</param>
public record DeviceChildrenResult(bool IsDeviceRegistered, List<ChildAccountViewModel> Children, string? ErrorMessage = null);
