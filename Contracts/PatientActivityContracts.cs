namespace AphaisaReverbes.Contracts;

public sealed record CreatePatientActivityRequest(
    string ActivityName,
    int Score,
    int Duration
);

public sealed record PatientActivityResponse(
    Guid Id,
    Guid PatientId,
    string ActivityName,
    int Score,
    int Duration,
    DateTimeOffset CreatedAt
);

