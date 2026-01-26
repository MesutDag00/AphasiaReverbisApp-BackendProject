namespace AphaisaReverbes.Models;

// Admin -> therapist kayıt daveti (tek kullanımlık; başarıyla kayıt olunca hard delete)
public sealed class TherapistInvitation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

