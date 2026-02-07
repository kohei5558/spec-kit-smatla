using GamifiedMathDrill.Api.DTOs;
using GamifiedMathDrill.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GamifiedMathDrill.Api.Controllers;

/// <summary>
/// 認証API
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// 保護者ログイン
    /// </summary>
    /// <param name="request">ログインリクエスト</param>
    /// <returns>ログインレスポンス</returns>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { Error = "Email and password are required" });
        }

        var (userId, displayName, role, parentId, token, expiresAt) = await _authService.LoginAsync(request.Email, request.Password, request.RememberMe);
        if (userId == null || token == null)
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
            return Unauthorized(new LoginResponse
            {
                Success = false,
                ErrorMessage = "メールアドレスまたはパスワードが間違っています"
            });
        }

        var response = new LoginResponse
        {
            Success = true,
            Token = token,
            UserId = userId,
            DisplayName = displayName!,
            Role = role!.Value.ToString(),
            ParentId = parentId,
            ExpiresAt = expiresAt!.Value
        };

        _logger.LogInformation("User {UserId} logged in successfully (RememberMe: {RememberMe}, ExpiresAt: {ExpiresAt})", 
            response.UserId, request.RememberMe, response.ExpiresAt);
        return Ok(response);
    }

    /// <summary>
    /// 子供ログイン
    /// </summary>
    /// <param name="request">子供ログインリクエスト</param>
    /// <returns>ログインレスポンス</returns>
    [HttpPost("child-login")]
    public async Task<ActionResult<LoginResponse>> ChildLogin([FromBody] ChildLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ChildId) || string.IsNullOrWhiteSpace(request.PIN))
        {
            return BadRequest(new { Error = "ChildId and PIN are required" });
        }

        var (userId, displayName, role, parentId, token, expiresAt) = await _authService.ChildLoginAsync(request.ChildId, request.PIN);
        if (userId == null || token == null)
        {
            _logger.LogWarning("Failed child login attempt for childId: {ChildId}", request.ChildId);
            return Unauthorized(new LoginResponse
            {
                Success = false,
                ErrorMessage = "子供IDまたはPINが間違っています"
            });
        }

        var response = new LoginResponse
        {
            Success = true,
            Token = token,
            UserId = userId,
            DisplayName = displayName!,
            Role = role!.Value.ToString(),
            ParentId = parentId,
            ExpiresAt = expiresAt!.Value
        };

        _logger.LogInformation("Child {UserId} logged in successfully", response.UserId);
        return Ok(response);
    }
}
