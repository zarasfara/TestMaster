using System.Timers;

namespace TestMaster.Presentation.Services;

public class ExamTimerService : IExamTimerService
{
    private System.Timers.Timer? _timer;
    private int _remainingSeconds;
    private int _totalSeconds;
    private bool _isRunning;

    public event Action<int>? OnTick;
    public event Action? OnTimeExpired;

    public int RemainingSeconds => _remainingSeconds;
    public bool IsRunning => _isRunning;
    
    /// <summary>
    /// Возвращает оставшееся время в формате HH:MM:SS
    /// </summary>
    public string FormattedTime => FormatTime(_remainingSeconds);
    
    /// <summary>
    /// Возвращает процент выполненного времени (0-100)
    /// </summary>
    public int ProgressPercent => _totalSeconds > 0 ? (int)(100 * (1 - _remainingSeconds / (double)_totalSeconds)) : 0;

    public void Start(int totalMinutes = 235)
    {
        if (_isRunning) return;

        _totalSeconds = totalMinutes * 60;
        _remainingSeconds = _totalSeconds;
        _isRunning = true;

        _timer = new System.Timers.Timer(1000); // 1 секунда
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
        _timer.Start();
    }

    /// <summary>
    /// Приостанавливает таймер
    /// </summary>
    public void Pause()
    {
        if (!_isRunning) return;
        _isRunning = false;
        _timer?.Stop();
    }

    /// <summary>
    /// Возобновляет таймер после паузы
    /// </summary>
    public void Resume()
    {
        if (_isRunning || _timer == null) return;
        _isRunning = true;
        _timer.Start();
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_remainingSeconds > 0)
        {
            _remainingSeconds--;
            OnTick?.Invoke(_remainingSeconds);
        }
        else
        {
            Stop();
            OnTimeExpired?.Invoke();
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
    }

    /// <summary>
    ///     Форматирует секунды в строку HH:MM:SS
    /// </summary>
    private static string FormatTime(int totalSeconds)
    {
        var ts = TimeSpan.FromSeconds(totalSeconds);
        return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
    }

    public void Dispose()
    {
        Stop();
    }
}