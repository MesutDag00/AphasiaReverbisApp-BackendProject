namespace AphaisaReverbes.Contracts;

public sealed record TherapistPatientListItemResponse(
    Guid Id,
    string FirstName,
    string LastName,
    int Age,
    int CityId,
    string CityName
);

