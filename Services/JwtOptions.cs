namespace AphaisaReverbes.Services;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "AphaisaReverbes";
    public string Audience { get; set; } = "AphaisaReverbes";

    // Symmetric key (>= 32 chars recommended)
    public string Key { get; set; } = "CHANGE_ME__USE_A_LONG_RANDOM_SECRET_32CHARS_MIN";

    public int ExpirationMinutes { get; set; } = 60 * 24;
}

