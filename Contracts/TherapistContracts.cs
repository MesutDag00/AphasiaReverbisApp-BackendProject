namespace AphaisaReverbes.Contracts;

using AphaisaReverbes.Models;

public sealed record RegisterTherapistRequest(
    string Code,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    DateTimeOffset GraduationDate,
    DateTimeOffset BirthDate,
    string Location
);

public sealed record TherapistResponse(
    Guid Id,
    string FirstName,
    string LastName,
    DateTimeOffset GraduationDate,
    DateTimeOffset BirthDate,
    string Location,
    DateTimeOffset CreatedAtUtc
);

public sealed record PatientSummaryResponse(
    Guid Id,
    string FirstName,
    string LastName,
    DateTimeOffset BirthDate,
    string Location,
    AphasiaType AphasiaType,
    DateTimeOffset CreatedAtUtc
);

public sealed record TherapistWithPatientsResponse(
    Guid Id,
    string FirstName,
    string LastName,
    DateTimeOffset GraduationDate,
    DateTimeOffset BirthDate,
    string Location,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyList<PatientSummaryResponse> Patients
);

public sealed record GeneratePatientInvitationRequest(AphasiaType AphasiaType);
public sealed record PatientInvitationResponse(string Code, AphasiaType AphasiaType, Guid TherapistId, DateTimeOffset CreatedAtUtc);

