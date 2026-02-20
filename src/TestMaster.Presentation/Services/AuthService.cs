namespace TestMaster.Presentation.Services;

public class AuthService : IAuthService
{
    private const string AdminUsername = "admin";
    private const string AdminPassword = "admin123";
    
    public bool IsAuthenticated { get; private set; }
    
    public event Action? OnAuthStateChanged;

    public Task<bool> LoginAsync(string username, string password)
    {
        if (username == AdminUsername && password == AdminPassword)
        {
            IsAuthenticated = true;
            OnAuthStateChanged?.Invoke();
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public void Logout()
    {
        IsAuthenticated = false;
        OnAuthStateChanged?.Invoke();
    }
}

