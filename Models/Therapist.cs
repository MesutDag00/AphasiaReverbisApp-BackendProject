namespace AphaisaReverbes.Models;

public sealed class Therapist : User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public DateTimeOffset GraduationDate { get; set; }
    public DateTimeOffset BirthDate { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public List<Patient> Patients { get; set; } = new();

}

