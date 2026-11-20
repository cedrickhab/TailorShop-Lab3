using TailorShop.Data;
using TailorShop.Models;

namespace TailorShop.Controllers;

public enum LoginResult { Success, Empty, Invalid, Locked }

public class AuthController
{
    public const int MaxAttempts = 3;
    // Failed attempts per username for this run only (lock lasts until restart).
    private readonly Dictionary<string, int> _failed = new(StringComparer.OrdinalIgnoreCase);

    public User CurrentUser { get; private set; }

    public bool IsLocked(string username) =>
        _failed.TryGetValue(username.Trim(), out var n) && n >= MaxAttempts;

    public LoginResult Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            return LoginResult.Empty;
        username = username.Trim();
        if (IsLocked(username)) return LoginResult.Locked;

        var user = FindUser(username);
        if (user == null || user.PasswordHash != Database.Hash(password))
        {
            _failed[username] = (_failed.TryGetValue(username, out var n) ? n : 0) + 1;
            if (user != null) SetFailedAttempts(user.Id, _failed[username]);
            return IsLocked(username) ? LoginResult.Locked : LoginResult.Invalid;
        }

        _failed[username] = 0;
        SetFailedAttempts(user.Id, 0);
        CurrentUser = user;
        return LoginResult.Success;
    }

    public void Logout() => CurrentUser = null;

    private static User FindUser(string username)
    {
        using var c = Database.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT Id, Username, PasswordHash, Role, FailedAttempts FROM Users WHERE Username = $u COLLATE NOCASE";
        cmd.Parameters.AddWithValue("$u", username);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new User
        {
            Id = r.GetInt64(0), Username = r.GetString(1), PasswordHash = r.GetString(2),
            Role = r.GetString(3), FailedAttempts = r.GetInt32(4)
        };
    }

    private static void SetFailedAttempts(long id, int n)
    {
        using var c = Database.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "UPDATE Users SET FailedAttempts = $n WHERE Id = $id";
        cmd.Parameters.AddWithValue("$n", n);
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }
}
