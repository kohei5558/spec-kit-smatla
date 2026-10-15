using System.Security.Claims;
using GamifiedMathDrill.Core.DTOs;
using GamifiedMathDrill.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamifiedMathDrill.Api.Controllers;

/// <summary>
/// 子供用端末の登録API
/// </summary>
[ApiController]
[Route("api/devices")]
[Authorize(Roles = "Parent")]
public class DevicesController : ControllerBase
{
    /// <summary>
    /// 端末トークンを送るHTTPヘッダー名
    /// </summary>
    public const string DeviceTokenHeader = "X-Device-Token";

    private readonly IDeviceService _deviceService;
    private readonly IChildAccountService _childAccountService;
    private readonly ILogger<DevicesController> _logger;

    public DevicesController(
        IDeviceService deviceService,
        IChildAccountService childAccountService,
        ILogger<DevicesController> logger)
    {
        _deviceService = deviceService;
        _childAccountService = childAccountService;
        _logger = logger;
    }

    /// <summary>
    /// ログイン中の端末を子供用に登録する（端末トークンはこのレスポンスでのみ返す）
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RegisterDeviceResponse>> RegisterAsync([FromBody] RegisterDeviceRequest request)
    {
        // 自動ModelState検証は無効化されているため明示的に検証する
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "端末名は1〜30文字で入力してください" });
        }

        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(parentId))
        {
            return Unauthorized();
        }

        try
        {
            var result = await _deviceService.RegisterAsync(parentId, request.Name);
            _logger.LogInformation("Device {DeviceId} registered", result.Device.Id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 登録済みの端末一覧を取得する
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<RegisteredDeviceDto>>> ListAsync()
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(parentId))
        {
            return Unauthorized();
        }

        return Ok(await _deviceService.ListAsync(parentId));
    }

    /// <summary>
    /// 端末の登録を解除する
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> RevokeAsync(int id)
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(parentId))
        {
            return Unauthorized();
        }

        if (!await _deviceService.RevokeAsync(parentId, id))
        {
            return NotFound(new { message = "端末が見つかりません" });
        }

        _logger.LogInformation("Device {DeviceId} revoked", id);
        return NoContent();
    }

    /// <summary>
    /// この端末を登録した家庭の、利用中の子供一覧を取得する（子供ログイン画面用、X-Device-Token 必須）
    /// </summary>
    [HttpGet("current/children")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ChildAccountDto>>> ListChildrenForDeviceAsync(
        [FromHeader(Name = DeviceTokenHeader)] string? deviceToken)
    {
        var parentId = await _deviceService.ResolveParentIdAsync(deviceToken);
        if (parentId == null)
        {
            return Unauthorized(new { message = "この端末は子供用に登録されていません" });
        }

        var children = await _childAccountService.ListAsync(parentId);
        return Ok(children.Where(c => c.IsActive).ToList());
    }
}
