using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Infrastructure.Data;

namespace GamifiedMathDrill.Infrastructure.Repositories;

public class LevelRepository : Repository<Level>, ILevelRepository
{
    public LevelRepository(ApplicationDbContext context) : base(context)
    {
    }
}
