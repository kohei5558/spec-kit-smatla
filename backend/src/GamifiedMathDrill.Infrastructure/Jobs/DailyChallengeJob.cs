using GamifiedMathDrill.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GamifiedMathDrill.Infrastructure.Jobs;

/// <summary>
/// Background service that creates a daily challenge every day at midnight
/// </summary>
public class DailyChallengeJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DailyChallengeJob> _logger;

    public DailyChallengeJob(
        IServiceProvider serviceProvider,
        ILogger<DailyChallengeJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DailyChallengeJob is starting");

        // Create initial challenge for today if it doesn't exist
        await CreateTodaysChallengeAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var tomorrow = now.Date.AddDays(1);
            var delay = tomorrow - now;

            _logger.LogInformation("Next daily challenge will be created at {Tomorrow} UTC (in {Delay})",
                tomorrow, delay);

            try
            {
                await Task.Delay(delay, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {
                    await CreateTodaysChallengeAsync();
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("DailyChallengeJob is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DailyChallengeJob");
                // Wait 1 hour before retrying on error
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        _logger.LogInformation("DailyChallengeJob has stopped");
    }

    private async Task CreateTodaysChallengeAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var challengeService = scope.ServiceProvider.GetRequiredService<IDailyChallengeService>();

            var today = DateTime.UtcNow.Date;
            var challenge = await challengeService.CreateDailyChallengeAsync(today);

            _logger.LogInformation(
                "Daily challenge created for {Date}: ProblemId={ProblemId}, BonusPoints={BonusPoints}",
                challenge.TargetDate, challenge.ProblemId, challenge.BonusPoints);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create daily challenge for today");
            throw;
        }
    }
}
