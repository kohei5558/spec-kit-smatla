using Blazored.LocalStorage;
using Blazored.SessionStorage;

namespace GamifiedMathDrill.Client.Services;

/// <summary>
/// JWTトークン管理サービス
/// セッションストレージ（rememberMe=false）またはローカルストレージ（rememberMe=true）を使用
/// </summary>
public class TokenService
{
    private readonly ILocalStorageService _localStorage;
    private readonly ISessionStorageService _sessionStorage;
    private const string TokenKey = "authToken";
    private const string UserIdKey = "userId";
    private const string UserNameKey = "userName";
    private const string UserRoleKey = "userRole";
    private const string ParentIdKey = "parentId";
    private const string TokenExpiryKey = "tokenExpiry";
    private const string RememberMeKey = "rememberMe";

    public TokenService(ILocalStorageService localStorage, ISessionStorageService sessionStorage)
    {
        _localStorage = localStorage;
        _sessionStorage = sessionStorage;
    }

    /// <summary>
    /// トークンとユーザー情報を保存
    /// rememberMe=trueの場合はローカルストレージ、falseの場合はセッションストレージを使用
    /// </summary>
    public async Task SaveTokenAsync(string token, string userId, string userName, string role, string? parentId, DateTime expiresAt, bool rememberMe)
    {
        var storage = rememberMe ? (object)_localStorage : _sessionStorage;
        
        if (storage is ILocalStorageService localStorage)
        {
            await localStorage.SetItemAsync(TokenKey, token);
            await localStorage.SetItemAsync(UserIdKey, userId);
            await localStorage.SetItemAsync(UserNameKey, userName);
            await localStorage.SetItemAsync(UserRoleKey, role);
            if (!string.IsNullOrEmpty(parentId))
            {
                await localStorage.SetItemAsync(ParentIdKey, parentId);
            }
            await localStorage.SetItemAsync(TokenExpiryKey, expiresAt.ToString("o"));
            await localStorage.SetItemAsync(RememberMeKey, rememberMe);
        }
        else if (storage is ISessionStorageService sessionStorage)
        {
            await sessionStorage.SetItemAsync(TokenKey, token);
            await sessionStorage.SetItemAsync(UserIdKey, userId);
            await sessionStorage.SetItemAsync(UserNameKey, userName);
            await sessionStorage.SetItemAsync(UserRoleKey, role);
            if (!string.IsNullOrEmpty(parentId))
            {
                await sessionStorage.SetItemAsync(ParentIdKey, parentId);
            }
            await sessionStorage.SetItemAsync(TokenExpiryKey, expiresAt.ToString("o"));
            await sessionStorage.SetItemAsync(RememberMeKey, rememberMe);
        }
    }

    /// <summary>
    /// トークンを取得（セッションストレージ優先、次にローカルストレージ）
    /// </summary>
    public async Task<string?> GetTokenAsync()
    {
        var token = await _sessionStorage.GetItemAsync<string>(TokenKey);
        if (!string.IsNullOrEmpty(token))
        {
            return token;
        }
        return await _localStorage.GetItemAsync<string>(TokenKey);
    }

    /// <summary>
    /// ユーザーIDを取得
    /// </summary>
    public async Task<string?> GetUserIdAsync()
    {
        var userId = await _sessionStorage.GetItemAsync<string>(UserIdKey);
        if (!string.IsNullOrEmpty(userId))
        {
            return userId;
        }
        return await _localStorage.GetItemAsync<string>(UserIdKey);
    }

    /// <summary>
    /// ユーザー名を取得
    /// </summary>
    public async Task<string?> GetUserNameAsync()
    {
        var userName = await _sessionStorage.GetItemAsync<string>(UserNameKey);
        if (!string.IsNullOrEmpty(userName))
        {
            return userName;
        }
        return await _localStorage.GetItemAsync<string>(UserNameKey);
    }

    /// <summary>
    /// ユーザーロールを取得
    /// </summary>
    public async Task<string?> GetUserRoleAsync()
    {
        var role = await _sessionStorage.GetItemAsync<string>(UserRoleKey);
        if (!string.IsNullOrEmpty(role))
        {
            return role;
        }
        return await _localStorage.GetItemAsync<string>(UserRoleKey);
    }

    /// <summary>
    /// 親IDを取得
    /// </summary>
    public async Task<string?> GetParentIdAsync()
    {
        var parentId = await _sessionStorage.GetItemAsync<string>(ParentIdKey);
        if (!string.IsNullOrEmpty(parentId))
        {
            return parentId;
        }
        return await _localStorage.GetItemAsync<string>(ParentIdKey);
    }

    /// <summary>
    /// トークンの有効期限を取得
    /// </summary>
    public async Task<DateTime?> GetTokenExpiryAsync()
    {
        var expiryStr = await _sessionStorage.GetItemAsync<string>(TokenExpiryKey);
        if (string.IsNullOrEmpty(expiryStr))
        {
            expiryStr = await _localStorage.GetItemAsync<string>(TokenExpiryKey);
        }
        
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
    /// トークンのみを保存する簡易メソッド（自動ログイン用）
    /// </summary>
    public async Task SetTokenAsync(string token, bool rememberMe)
    {
        var storage = rememberMe ? (object)_localStorage : _sessionStorage;
        
        if (storage is ILocalStorageService localStorage)
        {
            await localStorage.SetItemAsync(TokenKey, token);
            await localStorage.SetItemAsync(RememberMeKey, rememberMe);
        }
        else if (storage is ISessionStorageService sessionStorage)
        {
            await sessionStorage.SetItemAsync(TokenKey, token);
            await sessionStorage.SetItemAsync(RememberMeKey, rememberMe);
        }
    }

    /// <summary>
    /// トークンとユーザー情報を削除（両方のストレージから）
    /// </summary>
    public async Task RemoveTokenAsync()
    {
        await _localStorage.RemoveItemAsync(TokenKey);
        await _localStorage.RemoveItemAsync(UserIdKey);
        await _localStorage.RemoveItemAsync(UserNameKey);
        await _localStorage.RemoveItemAsync(UserRoleKey);
        await _localStorage.RemoveItemAsync(ParentIdKey);
        await _localStorage.RemoveItemAsync(TokenExpiryKey);
        await _localStorage.RemoveItemAsync(RememberMeKey);
        
        await _sessionStorage.RemoveItemAsync(TokenKey);
        await _sessionStorage.RemoveItemAsync(UserIdKey);
        await _sessionStorage.RemoveItemAsync(UserNameKey);
        await _sessionStorage.RemoveItemAsync(UserRoleKey);
        await _sessionStorage.RemoveItemAsync(ParentIdKey);
        await _sessionStorage.RemoveItemAsync(TokenExpiryKey);
        await _sessionStorage.RemoveItemAsync(RememberMeKey);
    }
}
