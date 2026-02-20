namespace TestMaster.Presentation.Services;

public sealed partial class ConnectivityService : IConnectivityService
{
    /// <summary>
    /// Проверяет интернет до восстановления подключения и выполняет действие
    /// </summary>
    public async Task CheckUntilConnectedAsync(Func<Task>? onConnected = null)
    {
        while (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            await Task.Delay(2000);
        }

        if (onConnected != null)
        {
            await onConnected();
        }
    }

    public void Dispose() { }
}

