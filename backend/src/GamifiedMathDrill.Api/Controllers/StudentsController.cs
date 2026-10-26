using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Core.Models.DTOs;
using GamifiedMathDrill.Core.Models.Responses;
using GamifiedMathDrill.Core.Services;
using GamifiedMathDrill.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamifiedMathDrill.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Parent,Child")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly IStudentAccessService _studentAccess;
    private readonly ILogger<StudentsController> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly ILevelRepository _levelRepository;

    public StudentsController(IStudentService studentService, IStudentAccessService studentAccess, ILogger<StudentsController> logger, TimeProvider timeProvider, ILevelRepository levelRepository)
    {
        _levelRepository = levelRepository;
        _studentService = studentService;
        _studentAccess = studentAccess;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> GetById(int id)
    {
        // 自分（子供）または自分の子供（保護者）の学習者のみ。他家庭の学習者は存在しない扱い
        if (!await this.CanAccessStudentAsync(_studentAccess, id))
        {
            return NotFound(ApiResponse<StudentDto>.ErrorResponse($"Student with ID {id} not found."));
        }

        var student = await _studentService.GetByIdAsync(id);
        if (student == null)
        {
            return NotFound(ApiResponse<StudentDto>.ErrorResponse($"Student with ID {id} not found."));
        }

        var studentDto = await MapToDtoAsync(student);
        return Ok(ApiResponse<StudentDto>.SuccessResponse(studentDto));
    }

    [HttpPatch("{id}/login")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> UpdateLogin(int id)
    {
        if (!await this.CanAccessStudentAsync(_studentAccess, id))
        {
            return NotFound(ApiResponse<StudentDto>.ErrorResponse($"Student with ID {id} not found."));
        }

        try
        {
            await _studentService.UpdateLoginAsync(id);
            // 連続日数の計算に学習記録が必要なため、読み直す
            var student = await _studentService.GetByIdAsync(id);
            var studentDto = await MapToDtoAsync(student!);
            return Ok(ApiResponse<StudentDto>.SuccessResponse(studentDto, "Login updated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<StudentDto>.ErrorResponse(ex.Message));
        }
    }

    private async Task<StudentDto> MapToDtoAsync(Student student)
    {
        var levels = StudentLevelRules.AllTypes(student, await _levelRepository.GetAllAsync());
        return new StudentDto
        {
            Id = student.Id,
            Name = student.Name,
            TotalPoints = student.TotalPoints,
            // 1問以上解いた日（日本時間）で数える。student は学習記録を含めて読み込んだもの
            ConsecutiveDays = StudyStreak.ConsecutiveDays(student.LearningRecords.Select(r => r.SolvedAt), JapanTime.Today(_timeProvider)),
            TotalProblems = student.TotalProblems,
            LastLoginAt = student.LastLoginAt,
            HighestLevel = levels.Max(l => l.Level.LevelNumber),
            Levels = levels.Select(l => new StudentLevelDto
            {
                CalculationType = l.Type.ToString(),
                LevelNumber = l.Level.LevelNumber,
                CorrectAnswers = l.CorrectAnswers,
                RequiredCorrectAnswers = l.Level.RequiredCorrectAnswers
            }).ToList()
        };
    }
}
