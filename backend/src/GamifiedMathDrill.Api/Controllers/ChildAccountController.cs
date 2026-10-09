using GamifiedMathDrill.Core.DTOs;
using GamifiedMathDrill.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GamifiedMathDrill.Api.Controllers;

/// <summary>
/// 子供アカウント管理API
/// </summary>
[ApiController]
[Route("api/child-accounts")]
[Authorize(Roles = "Parent")] // 子供アカウントの管理は保護者のみ
public class ChildAccountController : ControllerBase
{
    private readonly IChildAccountService _childAccountService;
    private readonly ILogger<ChildAccountController> _logger;

    public ChildAccountController(
        IChildAccountService childAccountService,
        ILogger<ChildAccountController> logger)
    {
        _childAccountService = childAccountService;
        _logger = logger;
    }

    /// <summary>
    /// 保護者の子供アカウント一覧を取得
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ChildAccountDto>>> ListAsync()
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(parentId))
        {
            return Unauthorized();
        }

        var children = await _childAccountService.ListAsync(parentId);
        return Ok(children);
    }

    /// <summary>
    /// 子供アカウント詳細を取得
    /// </summary>
    [HttpGet("{id}", Name = "GetChildAccount")]
    public async Task<ActionResult<ChildAccountDto>> GetAsync(string id)
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(parentId))
        {
            return Unauthorized();
        }

        var child = await _childAccountService.GetAsync(id, parentId);
        if (child == null)
        {
            return NotFound(new { message = "子供アカウントが見つかりません" });
        }

        return Ok(child);
    }

    /// <summary>
    /// 子供アカウント詳細（学習統計含む）を取得
    /// </summary>
    [HttpGet("{id}/detail")]
    public async Task<ActionResult<ChildAccountDto>> GetDetailAsync(string id)
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(parentId))
        {
            return Unauthorized();
        }

        var child = await _childAccountService.GetDetailAsync(id, parentId);
        if (child == null)
        {
            return NotFound(new { message = "子供アカウントが見つかりません" });
        }

        return Ok(child);
    }

    /// <summary>
    /// 子供アカウントを作成
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ChildAccountDto>> CreateAsync([FromBody] ChildAccountCreateDto dto)
    {
        // ModelState検証
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(parentId))
        {
            return Unauthorized();
        }

        try
        {
            // 重複PINチェック（警告のみ、作成は許可）
            var hasDuplicatePin = await _childAccountService.HasDuplicatePinAsync(parentId, dto.PIN);
            
            var child = await _childAccountService.CreateAsync(parentId, dto);

            // 重複PIN警告をレスポンスヘッダーに含める
            if (hasDuplicatePin)
            {
                Response.Headers["X-Pin-Warning"] = "他の子供と同じPINです。セキュリティ上推奨しません";
            }

            return Ok(child);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 子供アカウントを更新
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ChildAccountDto>> UpdateAsync(string id, [FromBody] ChildAccountUpdateDto dto)
    {
        // ModelState検証
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(parentId))
        {
            return Unauthorized();
        }

        try
        {
            // PIN変更時の重複チェック
            var hasDuplicatePin = false;
            if (!string.IsNullOrEmpty(dto.NewPIN))
            {
                hasDuplicatePin = await _childAccountService.HasDuplicatePinAsync(parentId, dto.NewPIN, id);
            }

            var child = await _childAccountService.UpdateAsync(id, parentId, dto);

            // 重複PIN警告をレスポンスヘッダーに含める
            if (hasDuplicatePin)
            {
                Response.Headers["X-Pin-Warning"] = "他の子供と同じPINです。セキュリティ上推奨しません";
            }

            return Ok(child);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 子供アカウントを一時停止
    /// </summary>
    [HttpPost("{id}/suspend")]
    public async Task<IActionResult> SuspendAsync(string id)
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(parentId))
        {
            return Unauthorized();
        }

        try
        {
            await _childAccountService.SuspendAsync(id, parentId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 子供アカウントを再開
    /// </summary>
    [HttpPost("{id}/activate")]
    public async Task<IActionResult> ActivateAsync(string id)
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(parentId))
        {
            return Unauthorized();
        }

        try
        {
            await _childAccountService.ActivateAsync(id, parentId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 子供アカウントを削除
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(parentId))
        {
            return Unauthorized();
        }

        try
        {
            await _childAccountService.DeleteAsync(id, parentId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
