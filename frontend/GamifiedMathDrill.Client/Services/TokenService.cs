using Blazored.LocalStorage;

namespace GamifiedMathDrill.Client.Services;

/// <summary>
/// JWTトークン管理サービス
/// </summary>
public class TokenService
{
    private readonly ILocalStorageService _localStorage;
    private const string TokenKey = "authToken";
    private const string UserIdKey = "userId";
    private const string UserNameKey = "userName";
    private const string UserRoleKey = "userRole";
    private const string ParentIdKey = "parentId";
    private const string TokenExpiryKey = "tokenExpiry";

    public TokenService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    /// <summary>
    /// トークンとユーザー情報を保存
    /// </summary>
    public async Task SaveTokenAsync(string token, string userId, string userName, string role, string? parentId, DateTime expiresAt)
    {
        await _localStorage.SetItemAsync(TokenKey, token);
        await _localStorage.SetItemAsync(UserIdKey, userId);
        await _localStorage.SetItemAsync(UserNameKey, userName);
        await _localStorage.SetItemAsync(UserRoleKey, role);
        if (!string.IsNullOrEmpty(parentId))
        {
            await _localStorage.SetItemAsync(ParentIdKey, parentId);
        }
        await _localStorage.SetItemAsync(TokenExpiryKey, expiresAt.ToString("o"));
    }

    /// <summary>
    /// トークンを取得
    /// </summary>
    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>(TokenKey);
    }

    /// <summary>
    /// ユーザーIDを取得
    /// </summary>
    public async Task<string?> GetUserIdAsync()
    {
        return await _localStorage.GetItemAsync<string>(UserIdKey);
    }

    /// <summary>
    /// ユーザー名を取得
    /// </summary>
    public async Task<string?> GetUserNameAsync()
    {
        return await _localStorage.GetItemAsync<string>(UserNameKey);
    }

    /// <summary>
    /// ユーザーロールを取得
    /// </summary>
    public async Task<string?> GetUserRoleAsync()
    {
        return await _localStorage.GetItemAsync<string>(UserRoleKey);
    }

    /// <summary>
    /// 親IDを取得
    /// </summary>
    public async Task<string?> GetParentIdAsync()
    {
        return await _localStorage.GetItemAsync<string>(ParentIdKey);
    }

    /// <summary>
    /// トークンの有効期限を取得
    /// </summary>
    public async Task<DateTime?> GetTokenExpiryAsync()
    {
        var expiryStr = await _localStorage.GetItemAsync<string>(TokenExpiryKey);
        if (string.IsNullOrEmpty(expiryStr))
        {
            return null;
        }
        return DateTime.Parse(expiryStr);
    }

    /// <summary>
    /// トークンが有効かどうかを確認
    /// </summary>
    public async Task<bool> IsTokenValidAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        var expiry = await GetTokenExpiryAsync();
        if (expiry == null || expiry.Value <= DateTime.UtcNow)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// トークンとユーザー情報を削除
    /// </summary>
    public async Task RemoveTokenAsync()
    {
        await _localStorage.RemoveItemAsync(TokenKey);
        await _localStorage.RemoveItemAsync(UserIdKey);
        await _localStorage.RemoveItemAsync(UserNameKey);
        await _localStorage.RemoveItemAsync(UserRoleKey);
        await _localStorage.RemoveItemAsync(ParentIdKey);
        await _localStorage.RemoveItemAsync(TokenExpiryKey);
    }
}
