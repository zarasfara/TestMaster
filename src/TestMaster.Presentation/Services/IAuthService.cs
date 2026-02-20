namespace TestMaster.Presentation.Services;

public interface IAuthService
{
    bool IsAuthenticated { get; }
    event Action? OnAuthStateChanged;
    Task<bool> LoginAsync(string username, string password);
    void Logout();
}

