namespace AphaisaReverbes.Models;

// Not an EF entity by itself; used for shared fields via inheritance.
public abstract class User
{
    public Gender Gender { get; set; } = Gender.Unknown;

    public string? PhoneNumber { get; set; }

    public int CityId { get; set; }
    public City City { get; set; } = null!;
}

