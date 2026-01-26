using Microsoft.EntityFrameworkCore;
using AphaisaReverbes.Data;
using AphaisaReverbes.Models;
using Microsoft.Extensions.Options;

namespace AphaisaReverbes.Services;

internal sealed class InvitationCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InvitationCleanupService> _logger;
    private readonly InvitationCleanupOptions _options;

    public InvitationCleanupService(
        IServiceScopeFactory scopeFactory,
        IOptions<InvitationCleanupOptions> options,
        ILogger<InvitationCleanupService> logger
    )
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run once shortly after startup, then periodically.
        var startupDelay = TimeSpan.FromSeconds(Math.Max(0, _options.StartupDelaySeconds));
        await Task.Delay(startupDelay, stoppingToken);

        var interval = TimeSpan.FromMinutes(Math.Max(1, _options.IntervalMinutes));
        using var timer = new PeriodicTimer(interval);
        do
        {
            try
            {
                await CleanupOnce(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // graceful shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invitation cleanup failed");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CleanupOnce(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var cutoff = DateTimeOffset.UtcNow - TimeSpan.FromDays(Math.Max(1, _options.MaxAgeDays));

        // Hard delete: unused invitations are those still in table after 7 days.
        // NOTE: EF Core SQLite has limited DateTimeOffset query translation; we fetch a lightweight projection
        // and filter in-memory, then RemoveRange as requested.
        var therapistCandidates = await db.TherapistInvitations
            .AsNoTracking()
            .Select(x => new { x.Id, x.CreatedAtUtc })
            .ToListAsync(ct);

        var patientCandidates = await db.PatientInvitations
            .AsNoTracking()
            .Select(x => new { x.Id, x.CreatedAtUtc })
            .ToListAsync(ct);

        var therapistToDelete = therapistCandidates
            .Where(x => x.CreatedAtUtc < cutoff)
            .Select(x => new TherapistInvitation { Id = x.Id, Code = string.Empty })
            .ToList();

        var patientToDelete = patientCandidates
            .Where(x => x.CreatedAtUtc < cutoff)
            .Select(x => new PatientInvitation { Id = x.Id, Code = string.Empty })
            .ToList();

        if (therapistToDelete.Count == 0 && patientToDelete.Count == 0)
            return;

        db.TherapistInvitations.RemoveRange(therapistToDelete);
        db.PatientInvitations.RemoveRange(patientToDelete);

        var deleted = await db.SaveChangesAsync(ct);
        _logger.LogInformation(
            "Invitation cleanup deleted {DeletedCount} rows (therapistInvites={TherapistCount}, patientInvites={PatientCount})",
            deleted,
            therapistToDelete.Count,
            patientToDelete.Count
        );
    }
}

