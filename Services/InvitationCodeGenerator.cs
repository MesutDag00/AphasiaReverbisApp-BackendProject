using System.Security.Cryptography;

namespace AphaisaReverbes.Services;

internal static class InvitationCodeGenerator
{
    public const int DefaultLength = 6;
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // okunması kolay (0/O/1/I yok)
    private static readonly HashSet<char> AlphabetSet = Alphabet.ToHashSet();

    public static string Generate()
        => Generate(DefaultLength);

    public static string? Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var code = input.Trim().ToUpperInvariant();
        if (code.Length != DefaultLength)
            return null;

        for (var i = 0; i < code.Length; i++)
        {
            if (!AlphabetSet.Contains(code[i]))
                return null;
        }

        return code;
    }

    public static string Generate(int length)
    {
        if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));

        // Spec: uppercase letters + digits, e.g. AB12C3
        Span<byte> bytes = stackalloc byte[length];
        RandomNumberGenerator.Fill(bytes);

        Span<char> chars = stackalloc char[length];
        for (var i = 0; i < length; i++)
        {
            chars[i] = Alphabet[bytes[i] % Alphabet.Length];
        }

        return new string(chars);
    }
}

