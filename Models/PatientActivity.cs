namespace AphaisaReverbes.Models;

public sealed class PatientActivity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }

    public string ActivityName { get; set; } = string.Empty;
    public int Score { get; set; }
    public int Duration { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

