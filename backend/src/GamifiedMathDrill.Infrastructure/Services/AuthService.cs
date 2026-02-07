using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GamifiedMathDrill.Infrastructure.Services;

/// <summary>
/// 認証サービスの実装
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    /// <summary>
    /// 保護者ログイン
    /// </summary>
    public async Task<(string? userId, string? displayName, UserRole? role, string? parentId, string? token, DateTime? expiresAt)> LoginAsync(string email, string password, bool rememberMe = false)
    {
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
}
