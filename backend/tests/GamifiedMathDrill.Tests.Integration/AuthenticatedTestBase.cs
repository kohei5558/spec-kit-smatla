using GamifiedMathDrill.Tests.Integration.Helpers;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// 認証が必要な統合テスト用のベースクラス
/// </summary>
public abstract class AuthenticatedTestBase : IClassFixture<TestWebApplicationFactory>
{
    protected readonly HttpClient Client;
    protected readonly TestWebApplicationFactory Factory;

    protected AuthenticatedTestBase(TestWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        
        // テストユーザーをシード
        AuthenticationHelper.EnsureTestUsersExistAsync(factory.Services).Wait();
    }

    /// <summary>
    /// 保護者として認証
    /// </summary>
    protected async Task<string> AuthenticateAsParentAsync()
    {
        var token = await AuthenticationHelper.LoginAsParentAsync(Client, Factory.Services);
        AuthenticationHelper.AddAuthorizationHeader(Client, token);
        return token;
    }

    /// <summary>
    /// 子供として認証
    /// </summary>
    protected async Task<string> AuthenticateAsChildAsync(string childId, string pin)
    {
        var token = await AuthenticationHelper.LoginAsChildAsync(Client, childId, pin, Factory.Services);
        AuthenticationHelper.AddAuthorizationHeader(Client, token);
        return token;
    }
}
