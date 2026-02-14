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
[Route("api/auth/child-login")]
public class ChildAuthController : ControllerBase
{
    private readonly IChildAccountService _childAccountService;
    private readonly IAuthService _authService;
    private readonly IStudentService _studentService;
    private readonly ILogger<ChildAuthController> _logger;

    public ChildAuthController(
        IChildAccountService childAccountService,
        IAuthService authService,
        IStudentService studentService,
        ILogger<ChildAuthController> logger)
    {
        _childAccountService = childAccountService;
        _authService = authService;
        _studentService = studentService;
        _logger = logger;
    }

    /// <summary>
    /// 子供ログイン（PINコード認証）
    /// </summary>
    [HttpPost("login")]
    [HttpPost("/api/auth/child-login")]
    public async Task<ActionResult<LoginResponse>> LoginAsync([FromBody] ChildLoginRequest request)
    {
        _logger.LogInformation("Child login request received for ChildId {ChildId}", request.ChildAccountId);

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
            _logger.LogWarning("Child account is locked out");
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
            _logger.LogWarning("Invalid PIN attempt for child account");
            await _childAccountService.RecordFailedPinAttemptAsync(request.ChildAccountId);
            
            return Unauthorized(new LoginResponse
            {
                Success = false,
                ErrorMessage = "PINが間違っています"
            });
        }

        // StudentIdが0の場合、自動的にStudentを作成して紐付ける
        if (child.StudentId == 0)
        {
            _logger.LogInformation("Creating Student for child account");
            
            var newStudent = new GamifiedMathDrill.Core.Models.Student
            {
                Name = child.Name,
                TotalPoints = 0,
                CurrentLevelId = 1,
                CorrectAnswers = 0,
                ConsecutiveDays = 0,
                LastLoginAt = DateTime.UtcNow
            };
            
            var student = await _studentService.CreateAsync(newStudent);
            
            // ChildAccountにStudentIdを設定
            await _childAccountService.UpdateStudentIdAsync(request.ChildAccountId, student.Id);
            child.StudentId = student.Id;
            
            _logger.LogInformation("Student created and linked to child account");
        }

        // JWT トークン生成（既存のAuthServiceを再利用）
        var (token, expiresAt) = await _authService.GenerateChildTokenAsync(request.ChildAccountId, child.Name);

        _logger.LogInformation("Child account logged in successfully");

        return Ok(new LoginResponse
        {
            Success = true,
            UserId = request.ChildAccountId,
            DisplayName = child.Name,
            Role = "Child",
            Token = token,
            ExpiresAt = expiresAt,
            StudentId = child.StudentId  // Student IDを追加
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
    /// テストのペイロードでは `ChildId` を使っているため、JSONプロパティ名を合わせる
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("ChildId")]
    public string ChildAccountId { get; set; } = string.Empty;

    /// <summary>
    /// PINコード（4桁）
    /// </summary>
    public string PIN { get; set; } = string.Empty;
}
