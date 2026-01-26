namespace AphaisaReverbes.Models;

public sealed class Patient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public DateTimeOffset BirthDate { get; set; }
    public string Location { get; set; } = string.Empty;

    public AphasiaType AphasiaType { get; set; } = AphasiaType.Unknown;

    // Davet kodu ile geldiyse therapistId atanır (optional)
    public Guid? TherapistId { get; set; }
    public Therapist? Therapist { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

