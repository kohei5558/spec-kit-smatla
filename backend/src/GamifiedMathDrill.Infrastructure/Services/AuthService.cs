using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace GamifiedMathDrill.Infrastructure.Services;

/// <summary>
/// 認証サービスの実装
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IEmailService emailService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _configuration = configuration;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// 保護者ログイン
    /// </summary>
    public async Task<(string? userId, string? displayName, UserRole? role, string? parentId, string? token, DateTime? expiresAt)> LoginAsync(string email, string password, bool rememberMe = false)
    {
        // 基本的な入力バリデーション
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return (null, null, null, null, null, null);
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !user.IsActive || user.Role != UserRole.Parent)
        {
            return (null, null, null, null, null, null);
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!isPasswordValid)
        {
            return (null, null, null, null, null, null);
        }

        // rememberMeに基づいて有効期限を設定
        var expiryMinutes = rememberMe
            ? _configuration.GetValue<int>("Jwt:RememberMeExpiryMinutes", 43200) // 30日間
            : _configuration.GetValue<int>("Jwt:SessionExpiryMinutes", 60);      // 60分

        var token = GenerateJwtToken(user.Id, user.DisplayName, user.Role.ToString(), user.ParentId, expiryMinutes);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        return (user.Id, user.DisplayName, user.Role, user.ParentId, token, expiresAt);
    }

    /// <summary>
    /// 子供ログイン
    /// </summary>
    public async Task<(string? userId, string? displayName, UserRole? role, string? parentId, string? token, DateTime? expiresAt)> ChildLoginAsync(string childId, string pin)
    {
        var user = await _userManager.FindByIdAsync(childId);
        if (user == null || !user.IsActive || user.Role != UserRole.Child)
        {
            return (null, null, null, null, null, null);
        }

        // PINの検証（ハッシュ化されたPINと比較）
        if (string.IsNullOrEmpty(user.PIN))
        {
            return (null, null, null, null, null, null);
        }

        var verificationResult = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PIN, pin);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return (null, null, null, null, null, null);
        }

        var token = GenerateJwtToken(user.Id, user.DisplayName, user.Role.ToString(), user.ParentId);
        var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryMinutes", 60);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        return (user.Id, user.DisplayName, user.Role, user.ParentId, token, expiresAt);
    }

    /// <summary>
    /// JWTトークンを生成
    /// </summary>
    public string GenerateJwtToken(string userId, string displayName, string role, string? parentId = null, int? expiryMinutes = null)
    {
        var secretKey = _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is not configured");
        var issuer = _configuration["Jwt:Issuer"] ?? "GamifiedMathDrill.Api";
        var audience = _configuration["Jwt:Audience"] ?? "GamifiedMathDrill.Client";
        var expiry = expiryMinutes ?? _configuration.GetValue<int>("Jwt:ExpiryMinutes", 60);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, displayName),
            new Claim(ClaimTypes.Role, role)
        };

        if (!string.IsNullOrEmpty(parentId))
        {
            claims.Add(new Claim("ParentId", parentId));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiry),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// パスワードリセットメールを送信
    /// </summary>
    public async Task<bool> SendPasswordResetEmailAsync(string email, string ipAddress)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);

            // セキュリティ上、ユーザーが存在しない場合でも成功を返す
            if (user == null || user.Role != UserRole.Parent)
            {
                _logger.LogWarning("Password reset requested for non-existent or non-parent email: {Email}", email);
                return true;
            }

            // トークンを生成
            var token = Guid.NewGuid().ToString("N");
            var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

            // トークンをDBに保存
            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(1), // 1時間有効
                IsUsed = false,
                IpAddress = ipAddress
            };

            await _passwordResetTokenRepository.CreateAsync(resetToken);

            // リセットリンクを生成
            var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var resetLink = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(token)}";

            // メール送信
            await _emailService.SendPasswordResetEmailAsync(email, resetLink);

            _logger.LogInformation("Password reset email sent to {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
            return false;
        }
    }

    /// <summary>
    /// パスワードリセットトークンを検証
    /// </summary>
    public async Task<bool> ValidateResetTokenAsync(string token)
    {
        try
        {
            var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
            var resetToken = await _passwordResetTokenRepository.GetByTokenHashAsync(tokenHash);

            if (resetToken == null)
            {
                _logger.LogWarning("Invalid reset token attempted");
                return false;
            }

            if (resetToken.IsUsed)
            {
                _logger.LogWarning("Already used reset token attempted: {TokenId}", resetToken.Id);
                return false;
            }

            if (resetToken.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Expired reset token attempted: {TokenId}", resetToken.Id);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate reset token");
            return false;
        }
    }

    /// <summary>
    /// パスワードをリセット
    /// </summary>
    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        try
        {
            var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
            var resetToken = await _passwordResetTokenRepository.GetByTokenHashAsync(tokenHash);

            if (resetToken == null || resetToken.IsUsed || resetToken.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid, used, or expired reset token attempted");
                return false;
            }

            // ユーザーを取得
            var user = await _userManager.FindByIdAsync(resetToken.UserId);
            if (user == null)
            {
                _logger.LogError("User not found for reset token: {UserId}", resetToken.UserId);
                return false;
            }

            // パスワードをリセット
            var removePasswordResult = await _userManager.RemovePasswordAsync(user);
            if (!removePasswordResult.Succeeded)
            {
                _logger.LogError("Failed to remove old password for user: {UserId}", user.Id);
                return false;
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(user, newPassword);
            if (!addPasswordResult.Succeeded)
            {
                _logger.LogError("Failed to add new password for user: {UserId}. Errors: {Errors}",
                    user.Id, string.Join(", ", addPasswordResult.Errors.Select(e => e.Description)));
                return false;
            }

            // トークンを使用済みとしてマーク
            resetToken.IsUsed = true;
            resetToken.UsedAt = DateTime.UtcNow;
            await _passwordResetTokenRepository.UpdateAsync(resetToken);

            _logger.LogInformation("Password reset successful for user: {UserId}", user.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset password");
            return false;
        }
    }

    /// <summary>
    /// 新規保護者アカウントを作成
    /// </summary>
    public async Task<(bool success, string? userId, string? token, DateTime? expiresAt, string? errorMessage)> RegisterAsync(
        string email,
        string displayName,
        string password,
        string confirmPassword)
    {
        try
        {
            // 基本的な入力バリデーション
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(displayName) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                return (false, null, null, null, "すべての項目を入力してください");
            }

            // パスワードの一致確認
            if (password != confirmPassword)
            {
                _logger.LogWarning("Registration failed: Passwords do not match for email: {Email}", email);
                return (false, null, null, null, "パスワードが一致しません");
            }

            // パスワードの基本バリデーション（8文字以上）
            if (password.Length < 8)
            {
                _logger.LogWarning("Registration failed: Password too short for email: {Email}", email);
                return (false, null, null, null, "パスワードは8文字以上で、大文字、小文字、数字、記号を含む必要があります");
            }

            // メールアドレスの重複チェック
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed: Email already exists: {Email}", email);
                return (false, null, null, null, "このメールアドレスは既に使用されています");
            }

            // 新しい保護者ユーザーを作成
            var newUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                DisplayName = displayName,
                Role = UserRole.Parent,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                EmailConfirmed = false // 実際の実装ではメール確認プロセスを追加する
            };

            // ユーザーを作成（パスワード強度検証は自動的に実施される）
            var result = await _userManager.CreateAsync(newUser, password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Registration failed: {Errors} for email: {Email}", errors, email);

                // パスワード強度エラーをユーザーフレンドリーに変換
                if (errors.Contains("Passwords must") || errors.Contains("パスワードは"))
                {
                    return (false, null, null, null, "パスワードは8文字以上で、大文字、小文字、数字、記号を含む必要があります");
                }

                return (false, null, null, null, errors);
            }

            // 自動ログイン用のJWTトークンを生成（デフォルトの60分有効期限）
            var token = GenerateJwtToken(newUser.Id, newUser.DisplayName, UserRole.Parent.ToString());
            var expiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:SessionExpiryMinutes", 60));

            _logger.LogInformation("User registered successfully: {UserId}, Email: {Email}", newUser.Id, email);

            return (true, newUser.Id, token, expiresAt, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register user with email: {Email}", email);
            return (false, null, null, null, "登録処理中にエラーが発生しました");
        }
    }
}
