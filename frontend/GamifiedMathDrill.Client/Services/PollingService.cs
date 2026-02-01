namespace GamifiedMathDrill.Client.Services;

/// <summary>
/// 30秒間隔でポーリングを行うサービス
/// </summary>
public class PollingService : IDisposable
{
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);
    private readonly Func<Task> _pollingAction;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _pollingTask;

    public event EventHandler? OnTick;

    public PollingService(Func<Task> pollingAction)
    {
        _pollingAction = pollingAction ?? throw new ArgumentNullException(nameof(pollingAction));
    }

    /// <summary>
    /// ポーリングを開始
    /// </summary>
    public void Start()
    {
        if (_pollingTask != null)
        {
            return; // Already running
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _pollingTask = Task.Run(async () =>
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await _pollingAction();
                    OnTick?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Polling error: {ex.Message}");
                }

                try
                {
                    await Task.Delay(_interval, _cancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }, _cancellationTokenSource.Token);
    }

    /// <summary>
    /// ポーリングを停止
    /// </summary>
    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
        _pollingTask?.Wait(TimeSpan.FromSeconds(5));
        _pollingTask = null;
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }

    public void Dispose()
    {
        Stop();
    }
}
