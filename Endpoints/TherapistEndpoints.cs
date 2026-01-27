using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AphaisaReverbes.Contracts;
using AphaisaReverbes.Data;
using AphaisaReverbes.Models;
using AphaisaReverbes.Services;

namespace AphaisaReverbes.Endpoints;

internal static class TherapistEndpoints
{
    public static RouteGroupBuilder MapTherapistEndpoints(this IEndpointRouteBuilder app)
    {
        // This is mapped under /api in ApiEndpoints.
        var group = app.MapGroup("/therapists").WithTags("Therapists");

        group.MapPost("/register", RegisterTherapist);
        group.MapGet("/", ListTherapists);
        group.MapGet("/{therapistId:guid}", GetTherapist);

        group.MapPost("/{therapistId:guid}/generate-code", GeneratePatientInvitationCode);
        group.MapGet("/{therapistId:guid}/pending-transfers", GetPendingTransfers)
            .RequireAuthorization("TherapistOnly");
        group.MapPut("/{therapistId:guid}/approve-transfer/{patientId:guid}", ApproveTransfer)
            .RequireAuthorization("TherapistOnly");

        group.MapGet("/pending-transfers", GetPendingTransfersSelf)
            .RequireAuthorization("TherapistOnly");
        group.MapPut("/approve-transfer/{patientId:guid}", ApproveTransferSelf)
            .RequireAuthorization("TherapistOnly");

        return group;
    }

    private static async Task<IResult> ListTherapists(AppDbContext db, CancellationToken ct)
    {
        // NOTE: SQLite doesn't support ordering by DateTimeOffset; order in-memory.
        var therapists = await db.Therapists
            .AsNoTracking()
            .Include(t => t.Patients)
            .ToListAsync(ct);

        var response = therapists
            .OrderByDescending(t => t.CreatedAtUtc)
            .Select(EndpointSupport.ToWithPatientsResponse)
            .ToList();

        return EndpointSupport.Ok(response);
    }

    private static async Task<IResult> GetTherapist(Guid therapistId, AppDbContext db, CancellationToken ct)
    {
        var therapist = await db.Therapists
            .AsNoTracking()
            .Include(t => t.Patients)
            .SingleOrDefaultAsync(t => t.Id == therapistId, ct);

        if (therapist is null)
            return EndpointSupport.NotFound("therapist bulunamadı.");

        return EndpointSupport.Ok(EndpointSupport.ToWithPatientsResponse(therapist));
    }

    private static async Task<IResult> RegisterTherapist(RegisterTherapistRequest request, AppDbContext db, CancellationToken ct)
    {
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

        email = email.ToLowerInvariant();
        var emailTaken = await db.Therapists.AsNoTracking().AnyAsync(t => t.Email == email, ct)
            || await db.Patients.AsNoTracking().AnyAsync(p => p.Email == email, ct);
        if (emailTaken)
            return EndpointSupport.BadRequest("email zaten kullanımda.");

        var graduationDate = request.GraduationDate;
        var birthDate = request.BirthDate;

        // Basic date validation (no future dates, reasonable minimum) + logical order.
        var nowUtc = DateTimeOffset.UtcNow;

        err = EndpointSupport.ValidateBirthDate(birthDate, nowUtc);
        if (err is not null) return err;

        err = EndpointSupport.ValidateGraduationDate(graduationDate, birthDate, nowUtc);
        if (err is not null) return err;

        var now = nowUtc;

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // Fetch invite (no tracking) + atomically delete by Code to prevent concurrent reuse.
        var inviteExists = await db.TherapistInvitations
            .AsNoTracking()
            .AnyAsync(x => x.Code == code, ct);

        if (!inviteExists)
            return EndpointSupport.BadRequest("Geçersiz kod.");

        var therapist = new Therapist
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = PasswordService.Hash(password),
            FirstName = firstName,
            LastName = lastName,
            GraduationDate = graduationDate,
            BirthDate = birthDate,
            Gender = request.Gender,
            PhoneNumber = phoneNumber,
            CityId = request.CityId,
            CreatedAtUtc = now
        };

        db.Therapists.Add(therapist);

        // Delete invite after therapist is prepared but before commit.
        // Raw SQL is used here to guarantee single-use under concurrency (SQLite provider constraints).
        var deletedInvites = await db.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM ""TherapistInvitations""
WHERE ""Code"" = {code};
", ct);

        if (deletedInvites != 1)
            return EndpointSupport.BadRequest("Geçersiz kod.");

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return EndpointSupport.Created($"/api/therapists/{therapist.Id}", EndpointSupport.ToResponse(therapist));
    }

    private static async Task<IResult> GeneratePatientInvitationCode(Guid therapistId, GeneratePatientInvitationRequest? request, AppDbContext db, CancellationToken ct)
    {
        var exists = await db.Therapists.AnyAsync(t => t.Id == therapistId, ct);
        if (!exists)
            return EndpointSupport.NotFound("therapist bulunamadı.");

        var now = DateTimeOffset.UtcNow;
        var aphasiaType = request?.AphasiaType ?? AphasiaType.Unknown;

        // Collision önleme: DB'de unique index var, yine de retry yapıyoruz.
        for (var attempt = 0; attempt < 40; attempt++)
        {
            var code = InvitationCodeGenerator.Generate();

            var entity = new PatientInvitation
            {
                Id = Guid.NewGuid(),
                Code = code,
                AphasiaType = aphasiaType,
                TherapistId = therapistId,
                CreatedAtUtc = now
            };

            db.PatientInvitations.Add(entity);

            try
            {
                await db.SaveChangesAsync(ct);
                return EndpointSupport.Created(
                    $"/api/therapists/{therapistId}/generate-code/{entity.Id}",
                    new PatientInvitationResponse(entity.Code, entity.AphasiaType, entity.TherapistId, entity.CreatedAtUtc)
                );
            }
            catch (DbUpdateException)
            {
                db.ChangeTracker.Clear();
            }
        }

        return EndpointSupport.Fail(StatusCodes.Status500InternalServerError, "Davet kodu üretilemedi. Lütfen tekrar deneyin.");
    }

    private static async Task<IResult> GetPendingTransfers(Guid therapistId, ClaimsPrincipal user, AppDbContext db, CancellationToken ct)
    {
        var userIdRaw = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdRaw, out var userId) || userId != therapistId)
            return Results.Forbid();

        var therapistExists = await db.Therapists
            .AsNoTracking()
            .AnyAsync(t => t.Id == therapistId, ct);

        if (!therapistExists)
            return EndpointSupport.NotFound("therapist bulunamadı.");

        // Patients who requested transfer to this therapist
        var patients = await db.Patients
            .AsNoTracking()
            .Where(p => p.TransferStatus == TransferStatus.Pending && p.TargetTherapistId == therapistId)
            .ToListAsync(ct);

        var response = patients
            .OrderByDescending(p => p.CreatedAtUtc)
            .Select(EndpointSupport.ToResponse)
            .ToList();

        return EndpointSupport.Ok(response);
    }

    private static async Task<IResult> ApproveTransfer(Guid therapistId, Guid patientId, ClaimsPrincipal user, AppDbContext db, CancellationToken ct)
    {
        var userIdRaw = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdRaw, out var userId) || userId != therapistId)
            return Results.Forbid();

        var therapistExists = await db.Therapists
            .AsNoTracking()
            .AnyAsync(t => t.Id == therapistId, ct);

        if (!therapistExists)
            return EndpointSupport.NotFound("therapist bulunamadı.");

        var patient = await db.Patients
            .SingleOrDefaultAsync(p => p.Id == patientId, ct);

        if (patient is null)
            return EndpointSupport.NotFound("patient bulunamadı.");

        if (patient.TransferStatus != TransferStatus.Pending || patient.TargetTherapistId != therapistId)
            return EndpointSupport.BadRequest("bekleyen transfer talebi bulunamadı.");

        patient.TherapistId = therapistId;
        patient.TargetTherapistId = null;
        patient.TransferStatus = TransferStatus.Approved;
        await db.SaveChangesAsync(ct);

        return EndpointSupport.Ok(EndpointSupport.ToResponse(patient));
    }

    private static Task<IResult> GetPendingTransfersSelf(ClaimsPrincipal user, AppDbContext db, CancellationToken ct)
    {
        var userIdRaw = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdRaw, out var therapistId))
            return Task.FromResult<IResult>(Results.Forbid());

        return GetPendingTransfers(therapistId, user, db, ct);
    }

    private static Task<IResult> ApproveTransferSelf(Guid patientId, ClaimsPrincipal user, AppDbContext db, CancellationToken ct)
    {
        var userIdRaw = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdRaw, out var therapistId))
            return Task.FromResult<IResult>(Results.Forbid());

        return ApproveTransfer(therapistId, patientId, user, db, ct);
    }
}

