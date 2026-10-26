using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GamifiedMathDrill.Tests.Integration.Helpers;

/// <summary>
/// 計算の種類ごとのレベル（014）を DB で直接読み書きする
/// </summary>
public static class StudentLevelTestHelper
{
    /// <summary>
    /// その種類のレベルと連続正解数。行が無ければ null
    /// </summary>
    public static async Task<(int Level, int CorrectAnswers)?> GetAsync(IServiceProvider services, int studentId, CalculationType type)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var row = await db.StudentLevels.AsNoTracking().FirstOrDefaultAsync(l => l.StudentId == studentId && l.CalculationType == type);
        return row == null ? null : (row.LevelId, row.CorrectAnswers);
    }

    /// <summary>
    /// 指定した種類のレベルと連続正解数を設定する（行が無ければ作る）
    /// </summary>
    public static async Task SetAsync(IServiceProvider services, int studentId, int level, int correctAnswers, params CalculationType[] types)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        foreach (var type in types)
        {
            var row = await db.StudentLevels.FirstOrDefaultAsync(l => l.StudentId == studentId && l.CalculationType == type);
            if (row == null)
            {
                row = new StudentLevel { StudentId = studentId, CalculationType = type };
                db.StudentLevels.Add(row);
            }
            row.LevelId = level;
            row.CorrectAnswers = correctAnswers;
        }
        await db.SaveChangesAsync();
    }
}
