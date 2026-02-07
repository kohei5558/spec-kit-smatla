# Technical Research: Authentication UI Pages

**Date**: 2026年2月7日  
**Feature**: [spec.md](spec.md) | [plan.md](plan.md)

## Research Summary

このドキュメントは、004-auth-ui-pagesの実装に必要な技術的決定事項を記録します。

## 1. セッション管理とJWTトークンの保持戦略

### Decision

「ログイン状態を保持する」チェックボックスで、ユーザーがセッション保持期間を選択できるようにする。

### Rationale

- **セキュリティ**: 共有デバイスでの自動ログインリスクを軽減
- **利便性**: 個人デバイスでの再ログイン不要
- **業界標準**: 多くの認証システムで採用されているパターン

### Implementation Approach

- **チェックボックスOFF（デフォルト）**: セッションストレージにJWT保存、ブラウザ終了で失効
- **チェックボックスON**: ローカルストレージにJWT保存、30日間有効
- JWT ExpiryMins設定：
  - セッション保持OFF: 60分
  - セッション保持ON: 43200分（30日）

### Alternatives Considered

- **常にセッション保持**: セキュリティリスクが高い（共有デバイス）
- **常にセッション終了**: 利便性が低い（毎回ログイン必要）

### Technical Details

```csharp
// Frontend: TokenService.cs
public async Task SaveTokenAsync(string token, string userId, string displayName,
    string role, string? parentId, DateTime expiresAt, bool rememberMe)
{
    if (rememberMe)
    {
        await _localStorage.SetItemAsync("authToken", token);
        // ... other data to localStorage
    }
    else
    {
        await _sessionStorage.SetItemAsync("authToken", token);
        // ... other data to sessionStorage
    }
}

// Backend: appsettings.json
{
  "Jwt": {
    "SessionExpiryMinutes": 60,
    "RememberMeExpiryMinutes": 43200
  }
}
```

---

## 2. パスワードリセットメール送信とエラーハンドリング

### Decision

メール送信が失敗した場合でも、常に成功メッセージを表示する。

### Rationale

- **セキュリティ**: アカウントの存在を推測される攻撃（User Enumeration Attack）を防止
- **UX**: ユーザーには一貫したフィードバック
- **監視**: 失敗は内部的にログに記録し、管理者が監視

### Implementation Approach

```csharp
// Backend: AuthService.cs
public async Task<bool> SendPasswordResetEmailAsync(string email)
{
    try
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // アカウントが存在しない場合もログのみ、UIには影響なし
            _logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
            return true; // Always return success
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"{_configuration["AppUrl"]}/reset-password?token={token}&email={email}";

        try
        {
            await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);
            _logger.LogInformation("Password reset email sent to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
            // Still return true to avoid revealing account existence
        }

        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error in SendPasswordResetEmailAsync for {Email}", email);
        return true; // Always return success for security
    }
}
```

### Alternatives Considered

- **エラーメッセージ表示**: セキュリティリスク（アカウント存在の漏洩）
- **詳細なエラー情報**: 技術的すぎる、ユーザー混乱の原因

---

## 3. 新規アカウント作成時の情報収集

### Decision

最小限の情報のみ収集：メールアドレス、表示名、パスワード。

### Rationale

- **MVP原則**: 必要最小限の機能で開始
- **離脱率低減**: フォーム項目が少ないほど完了率が高い
- **プライバシー**: 不要な個人情報を収集しない
- **拡張性**: 後からプロフィール編集画面で追加情報を収集可能

### Implementation Approach

```csharp
// DTO: RegisterRequest.cs
public class RegisterRequest
{
    [Required(ErrorMessage = "メールアドレスは必須です")]
    [EmailAddress(ErrorMessage = "有効なメールアドレスを入力してください")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "表示名は必須です")]
    [StringLength(50, ErrorMessage = "表示名は50文字以内で入力してください")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "パスワードは必須です")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "パスワードは8文字以上100文字以内で入力してください")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]",
        ErrorMessage = "パスワードは大文字、小文字、数字、記号を各1文字以上含む必要があります")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "パスワード確認は必須です")]
    [Compare(nameof(Password), ErrorMessage = "パスワードが一致しません")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
```

### Alternatives Considered

- **電話番号・住所を必須**: 離脱率が高まる、初期登録には不要
- **子供の人数を任意**: 後から子供アカウント作成時に自然に把握可能

---

## 4. ブルートフォース攻撃対策とレート制限

### Decision

IPアドレスベースのレート制限を実装（15分間に5回以上の失敗で15分間ブロック）。

### Rationale

- **セキュリティ**: 基本的なブルートフォース攻撃を防止
- **シンプルさ**: 完全なアカウントロック機能より実装が容易
- **正規ユーザー保護**: 正規ユーザーがロックアウトされるリスクなし（アカウント単位ではなくIP単位）

### Implementation Approach

```csharp
// Middleware: RateLimitMiddleware.cs
public class RateLimitMiddleware
{
    private static readonly ConcurrentDictionary<string, List<DateTime>> _loginAttempts = new();
    private const int MaxAttemptsPerWindow = 5;
    private static readonly TimeSpan WindowDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan BlockDuration = TimeSpan.FromMinutes(15);

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Path.StartsWithSegments("/api/auth/login"))
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            if (_loginAttempts.TryGetValue(ipAddress, out var attempts))
            {
                // Remove old attempts outside the window
                attempts.RemoveAll(t => t < DateTime.UtcNow - WindowDuration);

                if (attempts.Count >= MaxAttemptsPerWindow)
                {
                    var oldestAttempt = attempts.Min();
                    if (DateTime.UtcNow - oldestAttempt < BlockDuration)
                    {
                        context.Response.StatusCode = 429; // Too Many Requests
                        await context.Response.WriteAsJsonAsync(new
                        {
                            error = "しばらくしてから再度お試しください"
                        });
                        return;
                    }
                }
            }

            // Record this attempt after the request
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await next(context);

            if (context.Response.StatusCode == 401) // Unauthorized
            {
                _loginAttempts.AddOrUpdate(ipAddress,
                    new List<DateTime> { DateTime.UtcNow },
                    (key, existing) =>
                    {
                        existing.Add(DateTime.UtcNow);
                        return existing;
                    });
            }

            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        else
        {
            await next(context);
        }
    }
}
```

### Alternatives Considered

- **対策なし**: セキュリティリスクが高い
- **完全なアカウントロック**: 実装が複雑（ロック解除メール、管理者介入など）

---

## 5. パスワードリセットリンクの有効期限切れ処理

### Decision

有効期限切れのリンクにアクセスした場合、「リンクの有効期限が切れています」というメッセージとパスワードリセット画面への直接リンクを表示する。

### Rationale

- **UX**: ユーザーに状況を明確に伝える
- **導線**: 次のアクションへのスムーズな導線
- **シンプル**: 前回のメールアドレス自動入力は不要（セキュリティリスクあり）

### Implementation Approach

```razor
@* Frontend: ResetPassword.razor *@
@page "/reset-password"
@using GamifiedMathDrill.Client.Services
@inject AuthService AuthService
@inject NavigationManager Navigation

@if (tokenExpired)
{
    <div class="error-container">
        <MudIcon Icon="@Icons.Material.Filled.AccessTime" Size="Size.Large" Color="Color.Warning" />
        <MudText Typo="Typo.h5">リンクの有効期限が切れています</MudText>
        <MudText Typo="Typo.body1" Class="mt-2">
            パスワードリセットリンクは24時間で期限切れとなります。
        </MudText>
        <MudButton Variant="Variant.Filled" Color="Color.Primary"
                   Href="/forgot-password" Class="mt-4">
            新しいリセットリンクを送信する
        </MudButton>
    </div>
}
else
{
    @* Password reset form *@
}

@code {
    [SupplyParameterFromQuery]
    public string? Token { get; set; }

    private bool tokenExpired = false;

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrEmpty(Token))
        {
            Navigation.NavigateTo("/forgot-password");
            return;
        }

        var isValid = await AuthService.ValidateResetTokenAsync(Token);
        if (!isValid)
        {
            tokenExpired = true;
        }
    }
}
```

```csharp
// Backend: AuthService.cs
public async Task<bool> ValidateResetTokenAsync(string token, string email)
{
    var user = await _userManager.FindByEmailAsync(email);
    if (user == null) return false;

    // Check if token is valid and not expired
    var result = await _userManager.VerifyUserTokenAsync(user,
        _userManager.Options.Tokens.PasswordResetTokenProvider,
        "ResetPassword",
        token);

    return result;
}
```

### Alternatives Considered

- **一般的なエラーのみ**: ユーザーが次のアクションを理解しにくい
- **メールアドレス自動入力**: セキュリティリスク（URLパラメータからの情報漏洩）

---

## 6. メール送信サービスの選択

### Decision

ASP.NET Coreの`IEmailSender`インターフェースを実装し、SMTPまたはSendGridなどのサードパーティサービスを設定可能にする。

### Rationale

- **柔軟性**: 環境に応じてメールプロバイダーを切り替え可能
- **テスト容易性**: モック実装でテスト可能
- **業界標準**: ASP.NET Core Identity の標準パターン

### Implementation Approach

```csharp
// Interface: IEmailService.cs
public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
    Task SendWelcomeEmailAsync(string toEmail, string displayName);
}

// Implementation: EmailService.cs (SMTP example)
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        var smtpClient = new SmtpClient(_configuration["Email:SmtpHost"])
        {
            Port = int.Parse(_configuration["Email:SmtpPort"]),
            Credentials = new NetworkCredential(
                _configuration["Email:Username"],
                _configuration["Email:Password"]),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_configuration["Email:FromAddress"]),
            Subject = "パスワードリセットのご案内",
            Body = $@"
                <html>
                <body>
                    <p>パスワードリセットのリクエストを受け付けました。</p>
                    <p>以下のリンクをクリックして、新しいパスワードを設定してください：</p>
                    <p><a href='{resetLink}'>パスワードをリセット</a></p>
                    <p>このリンクは24時間有効です。</p>
                    <p>このメールに心当たりがない場合は、無視してください。</p>
                </body>
                </html>
            ",
            IsBodyHtml = true
        };
        mailMessage.To.Add(toEmail);

        try
        {
            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Password reset email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
            throw;
        }
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string displayName)
    {
        // Similar implementation for welcome email
    }
}
```

### Configuration

```json
{
  "Email": {
    "SmtpHost": "smtp.example.com",
    "SmtpPort": "587",
    "Username": "your-email@example.com",
    "Password": "your-password",
    "FromAddress": "noreply@gamifiedmathdrill.com"
  }
}
```

### Alternatives Considered

- **SendGrid SDK**: より高機能だが、ベンダーロックインのリスク
- **直接SMTP**: シンプルだが、スケーラビリティに課題

---

## 7. フロントエンド UI ライブラリとコンポーネント設計

### Decision

既存のMudBlazorを活用し、一貫したUIコンポーネントを使用する。

### Rationale

- **一貫性**: 既存アプリケーションとの統一感
- **生産性**: 既製のコンポーネントで開発速度向上
- **アクセシビリティ**: MudBlazorは標準でWCAG準拠

### Components to Use

- `MudTextField`: メールアドレス、パスワード入力
- `MudButton`: アクション実行（ログイン、登録、リセット）
- `MudAlert`: エラーメッセージ、成功メッセージ
- `MudCheckBox`: 「ログイン状態を保持する」
- `MudProgressCircular`: ローディング状態
- `MudCard`: フォームコンテナ

### Example

```razor
<MudCard Class="pa-4">
    <MudCardContent>
        <MudTextField @bind-Value="email" Label="メールアドレス"
                      Variant="Variant.Outlined" Required="true"
                      Validation="@(new EmailAddressAttribute())" />
        <MudTextField @bind-Value="password" Label="パスワード"
                      InputType="InputType.Password" Variant="Variant.Outlined"
                      Required="true" />
        <MudCheckBox @bind-Checked="rememberMe" Label="ログイン状態を保持する" />
        <MudButton Variant="Variant.Filled" Color="Color.Primary"
                   OnClick="HandleLogin" FullWidth="true" Disabled="@isLoading">
            @if (isLoading)
            {
                <MudProgressCircular Size="Size.Small" Indeterminate="true" />
                <span class="ml-2">ログイン中...</span>
            }
            else
            {
                <span>ログイン</span>
            }
        </MudButton>
    </MudCardContent>
</MudCard>
```

---

## 8. パスワード強度検証

### Decision

サーバーサイドとクライアントサイドの両方でパスワード強度を検証。

### Rationale

- **セキュリティ**: 弱いパスワードによるアカウント侵害を防止
- **UX**: クライアントサイド検証でリアルタイムフィードバック
- **防御**: サーバーサイド検証で最終確認

### Requirements

- 最小8文字
- 大文字を1文字以上
- 小文字を1文字以上
- 数字を1文字以上
- 記号を1文字以上

### Implementation

```csharp
// Backend: ASP.NET Core Identity Configuration
services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;
});
```

```razor
@* Frontend: Client-side validation *@
<MudTextField @bind-Value="password" Label="パスワード"
              InputType="InputType.Password"
              Validation="@(new Func<string, IEnumerable<string>>(ValidatePassword))" />

@code {
    private IEnumerable<string> ValidatePassword(string pwd)
    {
        if (string.IsNullOrWhiteSpace(pwd))
        {
            yield return "パスワードは必須です";
            yield break;
        }
        if (pwd.Length < 8)
            yield return "パスワードは8文字以上である必要があります";
        if (!pwd.Any(char.IsLower))
            yield return "小文字を1文字以上含む必要があります";
        if (!pwd.Any(char.IsUpper))
            yield return "大文字を1文字以上含む必要があります";
        if (!pwd.Any(char.IsDigit))
            yield return "数字を1文字以上含む必要があります";
        if (!pwd.Any(ch => !char.IsLetterOrDigit(ch)))
            yield return "記号を1文字以上含む必要があります";
    }
}
```

---

## Summary

すべての技術的決定事項を明確化しました。主なポイント：

1. **セッション管理**: ユーザー選択可能な保持期間（セッションストレージ vs ローカルストレージ）
2. **メール送信**: 失敗時も成功メッセージ表示（セキュリティ優先）
3. **情報収集**: 最小限（メール、表示名、パスワードのみ）
4. **レート制限**: IPベースで実装（15分/5回失敗でブロック）
5. **リンク有効期限**: 明確なエラーメッセージと再試行導線
6. **メールサービス**: 柔軟な設定可能な実装
7. **UIライブラリ**: MudBlazor活用
8. **パスワード強度**: サーバー/クライアント両方で検証

次のフェーズ（Phase 1）でデータモデルとAPI契約を定義します。
