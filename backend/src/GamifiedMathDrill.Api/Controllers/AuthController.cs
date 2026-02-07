using GamifiedMathDrill.Api.DTOs;
using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
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

    /// <summary>
    /// パスワードリセットメール送信
    /// </summary>
    /// <param name="request">パスワードリセットリクエスト</param>
    /// <returns>パスワードリセットレスポンス</returns>
    [HttpPost("forgot-password")]
    public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new ForgotPasswordResponse
            {
                Success = false,
                Message = "メールアドレスは必須です"
            });
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var success = await _authService.SendPasswordResetEmailAsync(request.Email, ipAddress);

        if (!success)
        {
            _logger.LogError("Failed to send password reset email to {Email}", request.Email);
            return StatusCode(500, new ForgotPasswordResponse
            {
                Success = false,
                Message = "メール送信に失敗しました"
            });
        }

        return Ok(new ForgotPasswordResponse
        {
            Success = true,
            Message = "パスワードリセットのメールを送信しました。メールをご確認ください。"
        });
    }

    /// <summary>
    /// パスワードリセットトークンを検証
    /// </summary>
    /// <param name="token">リセットトークン</param>
    /// <returns>トークン検証結果</returns>
    [HttpGet("validate-reset-token")]
    public async Task<ActionResult<Core.Models.Responses.ApiResponse<bool>>> ValidateResetToken([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new Core.Models.Responses.ApiResponse<bool>
            {
                Success = false,
                Message = "トークンは必須です",
                Data = false
            });
        }

        var isValid = await _authService.ValidateResetTokenAsync(token);

        return Ok(new Core.Models.Responses.ApiResponse<bool>
        {
            Success = true,
            Message = isValid ? "有効なトークンです" : "無効または期限切れのトークンです",
            Data = isValid
        });
    }

    /// <summary>
    /// パスワードをリセット
    /// </summary>
    /// <param name="request">パスワードリセットリクエスト</param>
    /// <returns>パスワードリセットレスポンス</returns>
    [HttpPost("reset-password")]
    public async Task<ActionResult<ResetPasswordResponse>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token) ||
            string.IsNullOrWhiteSpace(request.NewPassword) ||
            string.IsNullOrWhiteSpace(request.ConfirmPassword))
        {
            return BadRequest(new ResetPasswordResponse
            {
                Success = false,
                Message = "すべてのフィールドは必須です"
            });
        }

        if (request.NewPassword != request.ConfirmPassword)
        {
            return BadRequest(new ResetPasswordResponse
            {
                Success = false,
                Message = "パスワードが一致しません"
            });
        }

        var success = await _authService.ResetPasswordAsync(request.Token, request.NewPassword);

        if (!success)
        {
            _logger.LogWarning("Failed password reset attempt with token");
            return BadRequest(new ResetPasswordResponse
            {
                Success = false,
                Message = "無効または期限切れのトークンです"
            });
        }

        _logger.LogInformation("Password reset successful");
        return Ok(new ResetPasswordResponse
        {
            Success = true,
            Message = "パスワードがリセットされました"
        });
    }

    /// <summary>
    /// 新規保護者アカウントを作成
    /// </summary>
    /// <param name="request">登録リクエスト</param>
    /// <returns>登録レスポンス（自動ログイン用トークン含む）</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

        var (success, userId, token, expiresAt, errorMessage) = await _authService.RegisterAsync(
            request.Email,
            request.DisplayName,
            request.Password,
            request.ConfirmPassword
        );

        if (!success)
        {
            _logger.LogWarning("Registration failed for email {Email}: {Error}", request.Email, errorMessage);
            return BadRequest(new RegisterResponse
            {
                Success = false,
                Message = errorMessage
            });
        }

        _logger.LogInformation("User registered successfully: {UserId}", userId);
        return Ok(new RegisterResponse
        {
            Success = true,
            Message = "アカウントが作成されました",
            UserId = userId,
            Token = token,
            DisplayName = request.DisplayName,
            Email = request.Email,
            Role = UserRole.Parent.ToString(),
            ExpiresAt = expiresAt
        });
    }
}
