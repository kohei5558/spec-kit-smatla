using GamifiedMathDrill.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamifiedMathDrill.Api.Controllers;

/// <summary>
/// 景品の画像（DB に保存したもの、012）。&lt;img&gt; から読むためログイン不要。ID はランダムで推測できない
/// </summary>
[ApiController]
[Route("api/reward-images")]
[AllowAnonymous]
public class RewardImagesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RewardImagesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id)
    {
        var image = await _context.RewardImages.FindAsync(id);
        if (image == null)
        {
            return NotFound();
        }

        // 画像は作ったあと変わらない（差し替えると新しい ID になる）
        Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(image.Data, image.ContentType);
    }
}
