using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Services;

public class DailyChallengeService : IDailyChallengeService
{
    private readonly IDailyChallengeRepository _challengeRepository;
    private readonly IProblemRepository _problemRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILearningRecordRepository _learningRecordRepository;
    private readonly ILevelRepository _levelRepository;

    public DailyChallengeService(
        IDailyChallengeRepository challengeRepository,
        IProblemRepository problemRepository,
        IStudentRepository studentRepository,
        ILearningRecordRepository learningRecordRepository,
        ILevelRepository levelRepository)
    {
        _challengeRepository = challengeRepository;
        _problemRepository = problemRepository;
        _studentRepository = studentRepository;
        _learningRecordRepository = learningRecordRepository;
        _levelRepository = levelRepository;
    }

    public async Task<DailyChallenge?> GetTodaysChallengeAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var challenge = await _challengeRepository.GetByDateAsync(today);
        return challenge;
    }

    public async Task<(bool IsCorrect, int BonusPoints, bool LeveledUp, Level? NewLevel)> SubmitChallengeAnswerAsync(
        int studentId,
        int challengeId,
        int answer)
    {
        var challenge = await _challengeRepository.GetByIdAsync(challengeId);
        if (challenge == null)
        {
            throw new ArgumentException("Challenge not found", nameof(challengeId));
        }

        var problem = await _problemRepository.GetByIdAsync(challenge.ProblemId);
        if (problem == null)
        {
            throw new InvalidOperationException("Challenge problem not found");
        }

        var student = await _studentRepository.GetByIdAsync(studentId);
        if (student == null)
        {
            throw new ArgumentException("Student not found", nameof(studentId));
        }

        var isCorrect = answer == problem.CorrectAnswer;
        var bonusPoints = isCorrect ? challenge.BonusPoints : 0;

        // Create learning record
        var learningRecord = new LearningRecord
        {
            StudentId = studentId,
            ProblemId = problem.Id,
            IsCorrect = isCorrect,
            StudentAnswer = answer,
            PointsEarned = bonusPoints,
            TimeTakenSeconds = 0,
            SolvedAt = DateTime.UtcNow
        };

        await _learningRecordRepository.AddAsync(learningRecord);

        // Update student stats
        student.TotalProblems++;
        if (isCorrect)
        {
            student.CorrectAnswers++;
            student.TotalPoints += bonusPoints;
        }

        // Check for level up
        var currentLevel = await _levelRepository.GetByIdAsync(student.CurrentLevelId);
        var leveledUp = false;
        Level? newLevel = null;

        if (currentLevel != null && student.CorrectAnswers >= currentLevel.RequiredCorrectAnswers)
        {
            var nextLevel = await _levelRepository.GetByIdAsync(student.CurrentLevelId + 1);
            if (nextLevel != null)
            {
                student.CurrentLevelId = nextLevel.Id;
                student.CorrectAnswers = 0;
                leveledUp = true;
                newLevel = nextLevel;
            }
        }

        await _studentRepository.UpdateAsync(student);

        return (isCorrect, bonusPoints, leveledUp, newLevel);
    }

    public async Task<DailyChallenge> CreateDailyChallengeAsync(DateTime targetDate)
    {
        // Check if challenge already exists for this date
        var targetDateOnly = DateOnly.FromDateTime(targetDate.Date);
        var existingChallenge = await _challengeRepository.GetByDateAsync(targetDateOnly);
        if (existingChallenge != null)
        {
            return existingChallenge;
        }

        // Get a random difficult problem (difficulty 7-10)
        var problems = await _problemRepository.GetAllAsync();
        var difficultProblems = problems.Where(p => p.DifficultyLevel >= 7).ToList();

        if (!difficultProblems.Any())
        {
            throw new InvalidOperationException("No difficult problems available for daily challenge");
        }

        var random = new Random();
        var selectedProblem = difficultProblems[random.Next(difficultProblems.Count)];

        // Create challenge with bonus points (20-50 based on difficulty)
        var bonusPoints = selectedProblem.DifficultyLevel * 5;

        var challenge = new DailyChallenge
        {
            ProblemId = selectedProblem.Id,
            TargetDate = targetDateOnly,
            BonusPoints = bonusPoints,
            IsActive = true
        };

        return await _challengeRepository.AddAsync(challenge);
    }
}
