namespace TestMaster.Presentation.Services;

/// <summary>
/// Интерфейс для управления таймером экзамена
/// </summary>
public interface IExamTimerService : IDisposable
{
    /// <summary>
    /// Событие срабатывает каждую секунду с количеством оставшихся секунд
    /// </summary>
    event Action<int>? OnTick;

    /// <summary>
    /// Событие срабатывает когда время экзамена истекло
    /// </summary>
    event Action? OnTimeExpired;

    /// <summary>
    /// Оставшееся время в секундах
    /// </summary>
    int RemainingSeconds { get; }

    /// <summary>
    /// Работает ли таймер в данный момент
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Оставшееся время в формате HH:MM:SS
    /// </summary>
    string FormattedTime { get; }

    /// <summary>
    /// Процент выполненного времени (0-100)
    /// </summary>
    int ProgressPercent { get; }

    /// <summary>
    /// Запускает таймер на указанное количество минут
    /// </summary>
    /// <param name="totalMinutes">Общее время экзамена в минутах (по умолчанию 235)</param>
    void Start(int totalMinutes = 235);

    /// <summary>
    /// Приостанавливает таймер
    /// </summary>
    void Pause();

    /// <summary>
    /// Возобновляет таймер после паузы
    /// </summary>
    void Resume();

    /// <summary>
    /// Останавливает таймер и очищает ресурсы
    /// </summary>
    void Stop();
}

