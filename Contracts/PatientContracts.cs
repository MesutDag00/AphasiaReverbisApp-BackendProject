namespace AphaisaReverbes.Contracts;

using AphaisaReverbes.Models;

public sealed record RegisterPatientWithCodeRequest(
    string FirstName,
    string LastName,
    DateTimeOffset BirthDate,
    string Location,
    string Code
);

public sealed record ChangeTherapistRequest(Guid NewTherapistId);

public sealed record PatientResponse(
    Guid Id,
    string FirstName,
    string LastName,
    DateTimeOffset BirthDate,
    string Location,
    AphasiaType AphasiaType,
    Guid? TherapistId,
    TransferStatus TransferStatus,
    Guid? TargetTherapistId,
    DateTimeOffset CreatedAtUtc
);

