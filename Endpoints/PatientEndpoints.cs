using Microsoft.EntityFrameworkCore;
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
        return group;
    }

    private static async Task<IResult> RegisterWithCode(RegisterPatientWithCodeRequest request, AppDbContext db, CancellationToken ct)
    {
        var birthDate = request.BirthDate;
        var code = InvitationCodeGenerator.Normalize(request.Code);

        if (code is null)
            return EndpointSupport.BadRequest("Geçersiz kod.");

        if (!EndpointSupport.TryTrimRequired(request.FirstName, 100, "firstName", out var firstName, out var err))
            return err!;
        if (!EndpointSupport.TryTrimRequired(request.LastName, 100, "lastName", out var lastName, out err))
            return err!;
        if (!EndpointSupport.TryTrimRequired(request.Location, 200, "location", out var location, out err))
            return err!;

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
            FirstName = firstName,
            LastName = lastName,
            BirthDate = birthDate,
            Location = location,
            AphasiaType = invite.AphasiaType,
            TherapistId = invite.TherapistId,
            CreatedAtUtc = now
        };

        db.Patients.Add(patient);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return EndpointSupport.Created($"/api/patients/{patient.Id}", EndpointSupport.ToResponse(patient));
    }

    private static async Task<IResult> ListPatients(AppDbContext db, CancellationToken ct)
    {
        // NOTE: SQLite doesn't support ordering by DateTimeOffset; order in-memory.
        var patients = await db.Patients
            .AsNoTracking()
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
            .SingleOrDefaultAsync(x => x.Id == patientId, ct);

        if (patient is null)
            return EndpointSupport.NotFound("patient bulunamadı.");

        return EndpointSupport.Ok(EndpointSupport.ToResponse(patient));
    }
}

