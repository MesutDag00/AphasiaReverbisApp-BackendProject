namespace AphaisaReverbes.Models;

// Therapist -> patient kayıt daveti (tek kullanımlık; başarıyla kayıt olunca hard delete)
public sealed class PatientInvitation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;

    public AphasiaType AphasiaType { get; set; } = AphasiaType.Unknown;

    public Guid TherapistId { get; set; }
    public Therapist? Therapist { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

