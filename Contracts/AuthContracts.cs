namespace AphaisaReverbes.Contracts;

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(
    string Token,
    string Role,
    Guid Id,
    string Email,
    string FirstName,
    string LastName
);

