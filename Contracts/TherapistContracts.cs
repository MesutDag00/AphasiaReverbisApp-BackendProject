namespace AphaisaReverbes.Contracts;

using AphaisaReverbes.Models;

public sealed record TherapistRegisterDto(
    string Code,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    DateTimeOffset GraduationDate,
    DateTimeOffset BirthDate,
    Gender Gender,
    string? PhoneNumber,
    int CityId
);

public sealed record TherapistResponse(
    Guid Id,
    string FirstName,
    string LastName,
    DateTimeOffset GraduationDate,
    DateTimeOffset BirthDate,
    Gender Gender,
    string? PhoneNumber,
    int CityId,
    string CityName,
    DateTimeOffset CreatedAtUtc
);

public sealed record PatientSummaryResponse(
    Guid Id,
    string FirstName,
    string LastName,
    DateTimeOffset BirthDate,
    Gender Gender,
    string? PhoneNumber,
    int CityId,
    string CityName,
    AphasiaType AphasiaType,
    DateTimeOffset CreatedAtUtc
);

public sealed record TherapistWithPatientsResponse(
    Guid Id,
    string FirstName,
    string LastName,
    DateTimeOffset GraduationDate,
    DateTimeOffset BirthDate,
    Gender Gender,
    string? PhoneNumber,
    int CityId,
    string CityName,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyList<PatientSummaryResponse> Patients
);

public sealed record GeneratePatientInvitationRequest(AphasiaType AphasiaType);
public sealed record PatientInvitationResponse(string Code, AphasiaType AphasiaType, Guid TherapistId, DateTimeOffset CreatedAtUtc);

