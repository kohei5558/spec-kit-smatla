using GamifiedMathDrill.Core.DTOs;
using GamifiedMathDrill.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GamifiedMathDrill.Api.Controllers;

/// <summary>
/// プリセットアバターAPI
/// </summary>
[ApiController]
[Route("api/preset-avatars")]
public class PresetAvatarController : ControllerBase
{
    private readonly IPresetAvatarRepository _avatarRepository;

    public PresetAvatarController(IPresetAvatarRepository avatarRepository)
    {
        _avatarRepository = avatarRepository;
    }

    /// <summary>
    /// 全プリセットアバター一覧を取得
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<PresetAvatarDto>>> ListAsync()
    {
        var avatars = await _avatarRepository.GetAllAsync();
        
        var dtos = avatars.Select(a => new PresetAvatarDto
        {
            Id = a.Id,
            Name = a.Name,
            ImageUrl = $"/avatars/{a.FileName}",
            DisplayOrder = a.DisplayOrder
        }).ToList();

        return Ok(dtos);
    }
}
