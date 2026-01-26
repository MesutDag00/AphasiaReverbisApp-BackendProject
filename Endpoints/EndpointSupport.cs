using AphaisaReverbes.Contracts;
using AphaisaReverbes.Models;

namespace AphaisaReverbes.Endpoints;

internal static class EndpointSupport
{
    public static readonly DateTimeOffset MinDateUtc = new(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);

    // ---------- Responses ----------
    public static IResult Ok<T>(T data) => Results.Ok(ApiResponse<T>.Ok(data));
    public static IResult Created<T>(string location, T data) => Results.Created(location, ApiResponse<T>.Ok(data));

    public static IResult Fail(int statusCode, string message) =>
        Results.Json(ApiResponse<object>.Fail(message), statusCode: statusCode);

    public static IResult BadRequest(string message) => Fail(StatusCodes.Status400BadRequest, message);
    public static IResult NotFound(string message) => Fail(StatusCodes.Status404NotFound, message);

    // ---------- Validation ----------
    public static bool TryTrimRequired(string? input, int maxLen, string fieldName, out string value, out IResult? error)
    {
        value = (input ?? string.Empty).Trim();
        error = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            error = BadRequest($"{fieldName} zorunludur.");
            return false;
        }

        if (value.Length > maxLen)
        {
            error = BadRequest($"{fieldName} en fazla {maxLen} karakter olmalı.");
            return false;
        }

        return true;
    }

    public static IResult? ValidateBirthDate(DateTimeOffset birthDate, DateTimeOffset nowUtc)
    {
        if (birthDate < MinDateUtc || birthDate > nowUtc)
            return BadRequest("birthDate geçersiz.");
        return null;
    }

    public static IResult? ValidateGraduationDate(DateTimeOffset graduationDate, DateTimeOffset birthDate, DateTimeOffset nowUtc)
    {
        if (graduationDate < MinDateUtc || graduationDate > nowUtc)
            return BadRequest("graduationDate geçersiz.");
        if (graduationDate <= birthDate)
            return BadRequest("graduationDate geçersiz.");
        return null;
    }

    // ---------- DTO Mapping ----------
    public static TherapistResponse ToResponse(Therapist t) =>
        new(t.Id, t.FirstName, t.LastName, t.GraduationDate, t.BirthDate, t.Location, t.CreatedAtUtc);

    public static PatientResponse ToResponse(Patient p) =>
        new(p.Id, p.FirstName, p.LastName, p.BirthDate, p.Location, p.AphasiaType, p.TherapistId, p.CreatedAtUtc);

    public static PatientSummaryResponse ToSummary(Patient p) =>
        new(p.Id, p.FirstName, p.LastName, p.BirthDate, p.Location, p.AphasiaType, p.CreatedAtUtc);

    public static TherapistWithPatientsResponse ToWithPatientsResponse(Therapist t) =>
        new(
            t.Id,
            t.FirstName,
            t.LastName,
            t.GraduationDate,
            t.BirthDate,
            t.Location,
            t.CreatedAtUtc,
            t.Patients
                .OrderByDescending(p => p.CreatedAtUtc)
                .Select(ToSummary)
                .ToList()
        );
}

