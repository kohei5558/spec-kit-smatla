using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using System.Linq;

namespace GamifiedMathDrill.Core.Services;

/// <summary>
/// 保護者ダッシュボードサービス
/// </summary>
public class ParentDashboardService : IParentDashboardService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IExchangeRequestRepository _exchangeRequestRepository;
    private readonly IRewardRepository _rewardRepository;
    private readonly IStudentAccessService _studentAccess;

    public ParentDashboardService(
        IStudentRepository studentRepository,
        IExchangeRequestRepository exchangeRequestRepository,
        IRewardRepository rewardRepository,
        IStudentAccessService studentAccess)
    {
        _studentAccess = studentAccess;
        _studentRepository = studentRepository;
        _exchangeRequestRepository = exchangeRequestRepository;
        _rewardRepository = rewardRepository;
    }

    public async Task<DashboardSummary> GetDashboardSummaryAsync(string parentUserId)
    {
        // 子供一覧を取得
        var children = await GetChildrenAsync(parentUserId);

        // 未承認申請数を取得
        var pendingCount = await GetPendingRequestsCountAsync(parentUserId);

        // 最近の交換申請を取得
        var allRequests = await _exchangeRequestRepository.GetAllAsync();
        var childrenIds = children.Select(c => c.Id).ToList();

        // LINQの型推論を助けるため明示的に処理
        var filteredRequests = new List<ExchangeRequest>();
        foreach (var request in allRequests)
        {
            if (childrenIds.Contains(request.StudentId))
            {
                filteredRequests.Add(request);
            }
        }

        // RequestedAtで降順ソート
        filteredRequests.Sort((a, b) => b.RequestedAt.CompareTo(a.RequestedAt));

        var recentRequests = new List<ExchangeRequest>();
        for (int i = 0; i < Math.Min(5, filteredRequests.Count); i++)
        {
            recentRequests.Add(filteredRequests[i]);
        }

        // 景品統計を取得（自分の家庭の景品のみ）
        var allRewards = await _rewardRepository.GetAllAsync();
        var rewardsList = allRewards.Where(r => r.ParentId == parentUserId).ToList();
        var totalRewards = rewardsList.Count;
        var activeRewards = 0;
        foreach (var reward in rewardsList)
        {
            if (reward.IsActive)
            {
                activeRewards++;
            }
        }

        return new DashboardSummary
        {
            PendingRequestsCount = pendingCount,
            Children = children,
            RecentRequests = recentRequests,
            TotalRewardsCreated = totalRewards,
            ActiveRewardsCount = activeRewards
        };
    }

    public async Task<int> GetPendingRequestsCountAsync(string parentUserId)
    {
        // 保護者の子供を取得
        var children = await GetChildrenAsync(parentUserId);
        var childrenIds = new HashSet<int>();
        foreach (var child in children)
        {
            childrenIds.Add(child.Id);
        }

        // 子供たちの未承認申請を取得
        var allRequests = await _exchangeRequestRepository.GetAllAsync();
        var count = 0;
        foreach (var request in allRequests)
        {
            if (childrenIds.Contains(request.StudentId) && request.Status == ExchangeStatus.Pending)
            {
                count++;
            }
        }
        return count;
    }

    public async Task<List<Student>> GetChildrenAsync(string parentUserId)
    {
        // 子供アカウントに紐付いた学習者（Student.ParentUserId には子供自身のIDが入っており保護者では探せない）
        var childStudentIds = (await _studentAccess.GetChildrenStudentIdsAsync(parentUserId)).ToHashSet();
        var allStudents = await _studentRepository.GetAllAsync();
        var filtered = allStudents.Where(s => childStudentIds.Contains(s.Id)).ToList();

        // 名前でソート
        filtered.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
        return filtered;
    }

    public async Task<ParentStatistics> GetStatisticsAsync(string parentUserId)
    {
        // 子供一覧を取得
        var children = await GetChildrenAsync(parentUserId);
        var childrenIds = new HashSet<int>();
        foreach (var child in children)
        {
            childrenIds.Add(child.Id);
        }

        // 交換申請を取得
        var allRequests = await _exchangeRequestRepository.GetAllAsync();
        var childRequests = new List<ExchangeRequest>();
        foreach (var request in allRequests)
        {
            if (childrenIds.Contains(request.StudentId))
            {
                childRequests.Add(request);
            }
        }

        // 基本統計を計算
        var totalChildren = children.Count;
        var totalProblemsCompleted = 0;
        var totalPointsEarned = 0;
        foreach (var child in children)
        {
            totalProblemsCompleted += child.TotalProblems;
            totalPointsEarned += child.TotalPoints;
        }

        var totalExchangesApproved = 0;
        foreach (var request in childRequests)
        {
            if (request.Status == ExchangeStatus.Approved)
            {
                totalExchangesApproved++;
            }
        }

        // 今月の統計を計算
        var now = DateTime.UtcNow;
        var monthlyRequests = new List<ExchangeRequest>();
        foreach (var request in childRequests)
        {
            if (request.RequestedAt.Year == now.Year && request.RequestedAt.Month == now.Month)
            {
                monthlyRequests.Add(request);
            }
        }

        var monthlyExchangesApproved = 0;
        foreach (var request in monthlyRequests)
        {
            if (request.Status == ExchangeStatus.Approved)
            {
                monthlyExchangesApproved++;
            }
        }

        var monthlyStats = new MonthlyStats
        {
            Month = now.Month,
            Year = now.Year,
            ProblemsCompleted = 0, // TODO: 問題履歴が実装されたら集計
            PointsEarned = 0, // TODO: ポイント履歴が実装されたら集計
            ExchangesApproved = monthlyExchangesApproved
        };

        // 景品カテゴリー別内訳を計算
        var categoryMap = new Dictionary<string, (int count, int points)>();
        foreach (var request in childRequests)
        {
            if (request.Status == ExchangeStatus.Approved && request.Reward != null)
            {
                var category = request.Reward.Category.ToString();
                if (!categoryMap.ContainsKey(category))
                {
                    categoryMap[category] = (0, 0);
                }
                var current = categoryMap[category];
                categoryMap[category] = (current.count + 1, current.points + request.RequiredPoints);
            }
        }

        var rewardCategoryBreakdown = new List<RewardCategoryBreakdown>();
        foreach (var kvp in categoryMap)
        {
            rewardCategoryBreakdown.Add(new RewardCategoryBreakdown
            {
                Category = kvp.Key,
                TotalExchanges = kvp.Value.count,
                TotalPointsUsed = kvp.Value.points
            });
        }

        return new ParentStatistics
        {
            TotalChildren = totalChildren,
            TotalProblemsCompleted = totalProblemsCompleted,
            TotalPointsEarned = totalPointsEarned,
            TotalExchangesApproved = totalExchangesApproved,
            MonthlyStats = monthlyStats,
            CategoryBreakdown = new List<CategoryBreakdown>(), // TODO: 問題履歴が実装されたら集計
            RewardCategoryBreakdown = rewardCategoryBreakdown
        };
    }
}
