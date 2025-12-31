using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Core.Models.DTOs;
using GamifiedMathDrill.Core.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace GamifiedMathDrill.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(IStudentService studentService, ILogger<StudentsController> logger)
    {
        _studentService = studentService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> GetById(int id)
    {
        var student = await _studentService.GetByIdAsync(id);
        if (student == null)
        {
            return NotFound(ApiResponse<StudentDto>.ErrorResponse($"Student with ID {id} not found."));
        }

        var studentDto = MapToDto(student);
        return Ok(ApiResponse<StudentDto>.SuccessResponse(studentDto));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<StudentDto>>> Create([FromBody] CreateStudentDto createDto)
    {
        if (string.IsNullOrWhiteSpace(createDto.Name))
        {
            return BadRequest(ApiResponse<StudentDto>.ErrorResponse("Student name is required."));
        }

        if (createDto.Name.Length > 50)
        {
            return BadRequest(ApiResponse<StudentDto>.ErrorResponse("Student name must be 50 characters or less."));
        }

        var student = new Student
        {
            Name = createDto.Name.Trim()
        };

        var createdStudent = await _studentService.CreateAsync(student);
        var studentDto = MapToDto(createdStudent);

        return CreatedAtAction(
            nameof(GetById),
            new { id = createdStudent.Id },
            ApiResponse<StudentDto>.SuccessResponse(studentDto, "Student created successfully."));
    }

    [HttpPatch("{id}/login")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> UpdateLogin(int id)
    {
        try
        {
            var student = await _studentService.UpdateLoginAsync(id);
            var studentDto = MapToDto(student);
            return Ok(ApiResponse<StudentDto>.SuccessResponse(studentDto, "Login updated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<StudentDto>.ErrorResponse(ex.Message));
        }
    }

    private StudentDto MapToDto(Student student)
    {
        return new StudentDto
        {
            Id = student.Id,
            Name = student.Name,
            TotalPoints = student.TotalPoints,
            ConsecutiveDays = student.ConsecutiveDays,
            CorrectAnswers = student.CorrectAnswers,
            TotalProblems = student.TotalProblems,
            LastLoginAt = student.LastLoginAt,
            CurrentLevelId = student.CurrentLevelId,
            CurrentLevelName = $"Level {student.CurrentLevel?.LevelNumber}",
            CurrentLevelMinDifficulty = student.CurrentLevel?.MinDifficulty,
            CurrentLevelMaxDifficulty = student.CurrentLevel?.MaxDifficulty
        };
    }
}
