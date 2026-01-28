using AphaisaReverbes.Contracts;
using AphaisaReverbes.Data;
using Microsoft.EntityFrameworkCore;

namespace AphaisaReverbes.Services;

public sealed class TherapistService
{
    private readonly AppDbContext _db;

    public TherapistService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Returns assigned patients for the given therapist.
    /// If therapist does not exist, returns null.
    /// </summary>
    public async Task<IReadOnlyList<TherapistPatientListItemResponse>?> GetPatientsByTherapistId(Guid therapistId, CancellationToken ct)
    {
        var therapistExists = await _db.Therapists
            .AsNoTracking()
            .AnyAsync(t => t.Id == therapistId, ct);

        if (!therapistExists)
            return null;

        var nowDate = DateTimeOffset.UtcNow.UtcDateTime.Date;

        // Pull the minimal fields, compute Age in-memory (not translated to SQL reliably).
        var rows = await _db.Patients
            .AsNoTracking()
            .Where(p => p.TherapistId == therapistId)
            .Select(p => new
            {
                p.Id,
                p.FirstName,
                p.LastName,
                p.BirthDate,
                p.CityId,
                CityName = p.City != null ? p.City.Name : string.Empty
            })
            .ToListAsync(ct);

        static int ComputeAge(DateTime birthDate, DateTime nowDate)
        {
            var age = nowDate.Year - birthDate.Year;
            if (birthDate.Date > nowDate.AddYears(-age)) age--;
            return Math.Max(0, age);
        }

        var response = rows
            .Select(x =>
            {
                var birthDate = x.BirthDate.UtcDateTime.Date;
                var age = ComputeAge(birthDate, nowDate);
                return new TherapistPatientListItemResponse(x.Id, x.FirstName, x.LastName, age, x.CityId, x.CityName);
            })
            .ToList();

        return response;
    }
}

