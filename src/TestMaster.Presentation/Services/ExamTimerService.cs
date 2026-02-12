// Services/ExamTimerService.cs
using System.Timers;

namespace KEGE_Emulator.Services;

public class ExamTimerService : IDisposable
{
    private System.Timers.Timer? _timer;
    private int _remainingSeconds;
    private bool _isRunning = false;

    public event Action<int>? OnTick;
    public event Action? OnTimeExpired;

    public int RemainingSeconds => _remainingSeconds;
    public bool IsRunning => _isRunning;

    public void Start(int totalMinutes = 235)
    {
        if (_isRunning) return;

        _remainingSeconds = totalMinutes * 60;
        _isRunning = true;

        _timer = new System.Timers.Timer(1000); // 1 секунда
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
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

    public void Dispose()
    {
        Stop();
    }
}