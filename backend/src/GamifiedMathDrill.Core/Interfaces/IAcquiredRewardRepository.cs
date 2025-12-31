using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

public interface IAcquiredRewardRepository : IRepository<AcquiredReward>
{
    Task<List<AcquiredReward>> GetByStudentIdAsync(int studentId);
}
