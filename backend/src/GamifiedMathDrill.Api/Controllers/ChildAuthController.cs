using GamifiedMathDrill.Api.DTOs;
using GamifiedMathDrill.Core.DTOs;
using GamifiedMathDrill.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GamifiedMathDrill.Api.Controllers;

/// <summary>
/// 子供認証API
/// </summary>
[ApiController]
[Route("api/auth/child")]
public class ChildAuthController : ControllerBase
{
    private readonly IChildAccountService _childAccountService;
    private readonly IAuthService _authService;
    private readonly ILogger<ChildAuthController> _logger;

    public ChildAuthController(
        IChildAccountService childAccountService,
        IAuthService authService,
        ILogger<ChildAuthController> logger)
    {
        _childAccountService = childAccountService;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// 子供ログイン（PINコード認証）
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> LoginAsync([FromBody] ChildLoginRequest request)
    {
        if (string.IsNullOrEmpty(request.ChildAccountId) || string.IsNullOrEmpty(request.PIN))
        {
            return BadRequest(new { message = "子供アカウントIDとPINは必須です" });
        }

        // アカウント存在確認
        var child = await _childAccountService.GetAsync(request.ChildAccountId, ""); // parentId不要（子供自身のログイン）
        if (child == null)
        {
            return NotFound(new LoginResponse
            {
                Success = false,
                ErrorMessage = "アカウントが見つかりません"
            });
        }

        // IsActiveチェック（PIN検証前に実行して無駄な失敗カウントを防ぐ）
        if (!child.IsActive)
        {
            return StatusCode(403, new LoginResponse
            {
                Success = false,
                ErrorMessage = "アカウントは現在使用できません。保護者にお問い合わせください"
            });
        }

        // ロックアウトチェック
        var isLockedOut = await _childAccountService.IsLockedOutAsync(request.ChildAccountId);
        if (isLockedOut)
        {
            _logger.LogWarning("Child account {ChildId} is locked out", request.ChildAccountId);
            return StatusCode(429, new LoginResponse
            {
                Success = false,
                ErrorMessage = "3回間違えました。5分後に再度お試しください"
            });
        }

        // PIN検証
        var isValidPin = await _childAccountService.VerifyPinAsync(request.ChildAccountId, request.PIN);
        if (!isValidPin)
        {
            _logger.LogWarning("Invalid PIN attempt for child account {ChildId}", request.ChildAccountId);
            await _childAccountService.RecordFailedPinAttemptAsync(request.ChildAccountId);
            
            return Unauthorized(new LoginResponse
            {
                Success = false,
                ErrorMessage = "PINが間違っています"
            });
        }

        // PIN検証成功：失敗カウンターをクリア
        await _childAccountService.ClearFailedPinAttemptsAsync(request.ChildAccountId);

        // JWT トークン生成（既存のAuthServiceを再利用）
        var (token, expiresAt) = await _authService.GenerateChildTokenAsync(request.ChildAccountId, child.Name);

        _logger.LogInformation("Child account {ChildId} logged in successfully", request.ChildAccountId);

        return Ok(new LoginResponse
        {
            Success = true,
            UserId = request.ChildAccountId,
            DisplayName = child.Name,
            Role = "Child",
            Token = token,
            ExpiresAt = expiresAt
        });
    }
}

/// <summary>
/// 子供ログインリクエスト
/// </summary>
public class ChildLoginRequest
{
    /// <summary>
    /// 子供アカウントID
    /// </summary>
    public string ChildAccountId { get; set; } = string.Empty;

    /// <summary>
    /// PINコード（4桁）
    /// </summary>
    public string PIN { get; set; } = string.Empty;
}
