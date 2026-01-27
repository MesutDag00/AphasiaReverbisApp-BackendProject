namespace AphaisaReverbes.Models;

public sealed class Patient : User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public DateTimeOffset BirthDate { get; set; }

    public AphasiaType AphasiaType { get; set; } = AphasiaType.Unknown;

    // Davet kodu ile geldiyse therapistId atanır (optional)
    public Guid? TherapistId { get; set; }
    public Therapist? Therapist { get; set; }

    public TransferStatus TransferStatus { get; set; } = TransferStatus.None;
    public Guid? TargetTherapistId { get; set; }

    public ICollection<PatientActivity> Activities { get; set; } = new List<PatientActivity>();

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

