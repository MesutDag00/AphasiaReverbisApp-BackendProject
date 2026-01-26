namespace AphaisaReverbes.Services;

internal static class PasswordService
{
    public static string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);

    public static bool Verify(string password, string passwordHash)
        => BCrypt.Net.BCrypt.Verify(password, passwordHash);
}

