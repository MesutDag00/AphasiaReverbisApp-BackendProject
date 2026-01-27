using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AphaisaReverbes.Contracts;
using AphaisaReverbes.Data;
using AphaisaReverbes.Models;
using AphaisaReverbes.Services;

namespace AphaisaReverbes.Endpoints;

internal static class PatientEndpoints
{
    public static RouteGroupBuilder MapPatientEndpoints(this IEndpointRouteBuilder app)
    {
        // This is mapped under /api in ApiEndpoints.
        var group = app.MapGroup("/patients").WithTags("Patients");
        group.MapPost("/register-with-code", RegisterWithCode);
        group.MapGet("/", ListPatients);
        group.MapGet("/{patientId:guid}", GetPatient);
        group.MapPut("/{patientId:guid}/change-therapist", ChangeTherapist)
            .RequireAuthorization("PatientOnly");
        group.MapPut("/change-therapist", ChangeTherapistSelf)
            .RequireAuthorization("PatientOnly");

        group.MapPost("/activities", CreateActivity)
            .RequireAuthorization("PatientOnly");
        group.MapGet("/activities", ListMyActivities)
            .RequireAuthorization("PatientOnly");
        return group;
    }

    private static async Task<IResult> RegisterWithCode(PatientRegisterDto request, AppDbContext db, CancellationToken ct)
    {
        var birthDate = request.BirthDate;
        var code = InvitationCodeGenerator.Normalize(request.Code);

        if (code is null)
            return EndpointSupport.BadRequest("Geçersiz kod.");

        if (!EndpointSupport.TryTrimRequired(request.Email, 320, "email", out var email, out var err))
            return err!;
        if (!EndpointSupport.TryTrimRequired(request.Password, 200, "password", out var password, out err))
            return err!;
        if (!EndpointSupport.TryTrimRequired(request.FirstName, 100, "firstName", out var firstName, out err))
            return err!;
        if (!EndpointSupport.TryTrimRequired(request.LastName, 100, "lastName", out var lastName, out err))
            return err!;
        var phoneNumber = (request.PhoneNumber ?? string.Empty).Trim();
        if (phoneNumber.Length > 32)
            return EndpointSupport.BadRequest("phoneNumber en fazla 32 karakter olmalı.");
        if (phoneNumber.Length == 0)
            phoneNumber = null;

        if (!Enum.IsDefined(typeof(Gender), request.Gender))
            return EndpointSupport.BadRequest("gender geçersiz.");

        if (request.CityId <= 0)
            return EndpointSupport.BadRequest("cityId geçersiz.");

        var cityExists = await db.Cities.AsNoTracking().AnyAsync(c => c.Id == request.CityId, ct);
        if (!cityExists)
            return EndpointSupport.BadRequest("cityId geçersiz.");

        email = email.ToLowerInvariant();
        var emailTaken = await db.Therapists.AsNoTracking().AnyAsync(t => t.Email == email, ct)
            || await db.Patients.AsNoTracking().AnyAsync(p => p.Email == email, ct);
        if (emailTaken)
            return EndpointSupport.BadRequest("email zaten kullanımda.");

        var nowUtc = DateTimeOffset.UtcNow;
        err = EndpointSupport.ValidateBirthDate(birthDate, nowUtc);
        if (err is not null) return err;

        var now = nowUtc;

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // Read invite info first (no tracking), then atomically delete to prevent concurrent reuse.
        var invite = await db.PatientInvitations
            .AsNoTracking()
            .Where(x => x.Code == code)
            .Select(x => new { x.Id, x.TherapistId, x.AphasiaType })
            .SingleOrDefaultAsync(ct);

        if (invite is null)
            return EndpointSupport.BadRequest("Geçersiz kod.");

        var deletedInvites = await db.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM ""PatientInvitations""
WHERE ""Id"" = {invite.Id};
", ct);

        if (deletedInvites != 1)
            return EndpointSupport.BadRequest("Geçersiz kod.");

        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = PasswordService.Hash(password),
            FirstName = firstName,
            LastName = lastName,
            BirthDate = birthDate,
            Gender = request.Gender,
            PhoneNumber = phoneNumber,
            CityId = request.CityId,
            AphasiaType = invite.AphasiaType,
            TherapistId = invite.TherapistId,
            CreatedAtUtc = now
        };

        db.Patients.Add(patient);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        var created = await db.Patients
            .AsNoTracking()
            .Include(p => p.City)
            .SingleAsync(p => p.Id == patient.Id, ct);

        return EndpointSupport.Created($"/api/patients/{created.Id}", EndpointSupport.ToResponse(created));
    }

    private static async Task<IResult> ListPatients(AppDbContext db, CancellationToken ct)
    {
        // NOTE: SQLite doesn't support ordering by DateTimeOffset; order in-memory.
        var patients = await db.Patients
            .AsNoTracking()
            .Include(p => p.City)
            .ToListAsync(ct);

        var response = patients
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(EndpointSupport.ToResponse)
            .ToList();

        return EndpointSupport.Ok(response);
    }

    private static async Task<IResult> GetPatient(Guid patientId, AppDbContext db, CancellationToken ct)
    {
        var patient = await db.Patients
            .AsNoTracking()
            .Include(p => p.City)
            .SingleOrDefaultAsync(x => x.Id == patientId, ct);

        if (patient is null)
            return EndpointSupport.NotFound("patient bulunamadı.");

        return EndpointSupport.Ok(EndpointSupport.ToResponse(patient));
    }

    private static async Task<IResult> ChangeTherapist(Guid patientId, ChangeTherapistRequest request, ClaimsPrincipal user, AppDbContext db, CancellationToken ct)
    {
        var userIdRaw = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdRaw, out var userId) || userId != patientId)
            return Results.Forbid();

        if (request.NewTherapistId == Guid.Empty)
            return EndpointSupport.BadRequest("newTherapistId geçersiz.");

        var newTherapistExists = await db.Therapists
            .AsNoTracking()
            .AnyAsync(t => t.Id == request.NewTherapistId, ct);

        if (!newTherapistExists)
            return EndpointSupport.NotFound("therapist bulunamadı.");

        var patient = await db.Patients
            .SingleOrDefaultAsync(p => p.Id == patientId, ct);

        if (patient is null)
            return EndpointSupport.NotFound("patient bulunamadı.");

        if (patient.TherapistId == request.NewTherapistId)
            return EndpointSupport.BadRequest("patient zaten bu therapist'e bağlı.");

        patient.TargetTherapistId = request.NewTherapistId;
        patient.TransferStatus = TransferStatus.Pending;
        await db.SaveChangesAsync(ct);

        return EndpointSupport.Ok(EndpointSupport.ToResponse(patient));
    }

    private static Task<IResult> ChangeTherapistSelf(ChangeTherapistRequest request, ClaimsPrincipal user, AppDbContext db, CancellationToken ct)
    {
        var userIdRaw = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdRaw, out var userId))
            return Task.FromResult<IResult>(Results.Forbid());

        return ChangeTherapist(userId, request, user, db, ct);
    }

    private static async Task<IResult> CreateActivity(CreatePatientActivityRequest request, ClaimsPrincipal user, AppDbContext db, CancellationToken ct)
    {
        var userIdRaw = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdRaw, out var patientId))
            return Results.Forbid();

        if (!EndpointSupport.TryTrimRequired(request.ActivityName, 200, "activityName", out var activityName, out var err))
            return err!;
        if (request.Duration < 0)
            return EndpointSupport.BadRequest("duration geçersiz.");

        var exists = await db.Patients.AsNoTracking().AnyAsync(p => p.Id == patientId, ct);
        if (!exists)
            return EndpointSupport.NotFound("patient bulunamadı.");

        var now = DateTimeOffset.UtcNow;
        var entity = new PatientActivity
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            ActivityName = activityName,
            Score = request.Score,
            Duration = request.Duration,
            CreatedAt = now
        };

        db.PatientActivities.Add(entity);
        await db.SaveChangesAsync(ct);

        var response = new PatientActivityResponse(
            entity.Id,
            entity.PatientId,
            entity.ActivityName,
            entity.Score,
            entity.Duration,
            entity.CreatedAt
        );

        return EndpointSupport.Created($"/api/patients/activities/{entity.Id}", response);
    }

    private static async Task<IResult> ListMyActivities(ClaimsPrincipal user, AppDbContext db, CancellationToken ct)
    {
        var userIdRaw = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdRaw, out var patientId))
            return Results.Forbid();

        // NOTE: SQLite doesn't support ordering by DateTimeOffset; order in-memory.
        var activities = await db.PatientActivities
            .AsNoTracking()
            .Where(x => x.PatientId == patientId)
            .ToListAsync(ct);

        var response = activities
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PatientActivityResponse(x.Id, x.PatientId, x.ActivityName, x.Score, x.Duration, x.CreatedAt))
            .ToList();

        return EndpointSupport.Ok(response);
    }
}

