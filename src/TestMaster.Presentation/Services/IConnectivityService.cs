namespace TestMaster.Presentation.Services;

/// <summary>
/// Интерфейс для проверки подключения к интернету
/// </summary>
public interface IConnectivityService : IDisposable
{
    /// <summary>
    /// Проверяет интернет до восстановления подключения и выполняет действие
    /// </summary>
    /// <param name="onConnected">Действие при восстановлении интернета (опционально)</param>
    Task CheckUntilConnectedAsync(Func<Task>? onConnected = null);
}
