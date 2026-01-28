namespace AphaisaReverbes.Services;

internal static class RegistrationRules
{
    public const string MustBeAdultMessage = "Sisteme kayıt olabilmek için 18 yaşından büyük olmalısınız.";

    public static bool IsAdult(DateTimeOffset birthDate, DateTimeOffset nowUtc, int minimumAgeYears = 18)
    {
        // Accurate age calculation (handles birthdays).
        var age = nowUtc.Year - birthDate.Year;
        if (birthDate.UtcDateTime.Date > nowUtc.UtcDateTime.Date.AddYears(-age))
            age--;
        return age >= minimumAgeYears;
    }
}

