namespace AphaisaReverbes.Contracts;

using AphaisaReverbes.Models;

public sealed record RegisterPatientWithCodeRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    DateTimeOffset BirthDate,
    Gender Gender,
    string? PhoneNumber,
    int CityId,
    string Code
);

public sealed record ChangeTherapistRequest(Guid NewTherapistId);

public sealed record PatientResponse(
    Guid Id,
    string FirstName,
    string LastName,
    DateTimeOffset BirthDate,
    Gender Gender,
    string? PhoneNumber,
    int? CityId,
    AphasiaType AphasiaType,
    Guid? TherapistId,
    TransferStatus TransferStatus,
    Guid? TargetTherapistId,
    DateTimeOffset CreatedAtUtc
);

