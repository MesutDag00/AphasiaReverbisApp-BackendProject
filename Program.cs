using Microsoft.EntityFrameworkCore;
using AphaisaReverbes.Data;
using AphaisaReverbes.Endpoints;
using AphaisaReverbes.Services;
using System.IO;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Swagger (OpenAPI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// DB (SQLite)
builder.Services.AddDbContext<AppDbContext>(options =>
{
    // IMPORTANT: Use absolute path to avoid "different app.db per working directory"
    var dbPath = Path.Combine(builder.Environment.ContentRootPath, "app.db");
    options.UseSqlite($"Data Source={dbPath}");
});

builder.Services.Configure<InvitationCleanupOptions>(
    builder.Configuration.GetSection(InvitationCleanupOptions.SectionName)
);

builder.Services.AddHostedService<InvitationCleanupService>();

var app = builder.Build();

// DB init (şimdilik demo: migration yerine otomatik create)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // EnsureCreated mevcut DB'yi migrate etmez. Şema değiştiyse (ör. TenantSystems tablosu yoksa)
    // geliştirme ortamında en net çözüm: DB'yi yeniden oluşturmak.
    db.Database.EnsureCreated();

    try
    {
        var hasTherapistInvitations = db.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='TherapistInvitations';")
            .AsEnumerable()
            .FirstOrDefault() > 0;

        var hasPatientInvitations = db.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='PatientInvitations';")
            .AsEnumerable()
            .FirstOrDefault() > 0;

        var hasPatientActivities = db.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='PatientActivities';")
            .AsEnumerable()
            .FirstOrDefault() > 0;

        var hasAphasiaTypeColumnPatients = false;
        var hasAphasiaTypeColumnInvites = false;
        var hasTherapistProfileColumns = false;
        var hasPatientProfileColumns = false;

        if (hasTherapistInvitations && hasPatientInvitations)
        {
            var patientCols = db.Database
                .SqlQueryRaw<string>("SELECT name FROM pragma_table_info('Patients');")
                .AsEnumerable()
                .ToArray();

            var inviteCols = db.Database
                .SqlQueryRaw<string>("SELECT name FROM pragma_table_info('PatientInvitations');")
                .AsEnumerable()
                .ToArray();

            var therapistCols = db.Database
                .SqlQueryRaw<string>("SELECT name FROM pragma_table_info('Therapists');")
                .AsEnumerable()
                .ToArray();

            var therapistInviteCols = db.Database
                .SqlQueryRaw<string>("SELECT name FROM pragma_table_info('TherapistInvitations');")
                .AsEnumerable()
                .ToArray();

            hasAphasiaTypeColumnPatients = patientCols.Any(c => string.Equals(c, "AphasiaType", StringComparison.OrdinalIgnoreCase));
            hasAphasiaTypeColumnInvites = inviteCols.Any(c => string.Equals(c, "AphasiaType", StringComparison.OrdinalIgnoreCase));

            var hasTransferStatusColumnPatients = patientCols.Any(c => string.Equals(c, "TransferStatus", StringComparison.OrdinalIgnoreCase));
            var hasTargetTherapistIdColumnPatients = patientCols.Any(c => string.Equals(c, "TargetTherapistId", StringComparison.OrdinalIgnoreCase));

            hasTherapistProfileColumns =
                therapistCols.Any(c => string.Equals(c, "FirstName", StringComparison.OrdinalIgnoreCase)) &&
                therapistCols.Any(c => string.Equals(c, "LastName", StringComparison.OrdinalIgnoreCase)) &&
                therapistCols.Any(c => string.Equals(c, "GraduationDate", StringComparison.OrdinalIgnoreCase)) &&
                therapistCols.Any(c => string.Equals(c, "BirthDate", StringComparison.OrdinalIgnoreCase)) &&
                therapistCols.Any(c => string.Equals(c, "Location", StringComparison.OrdinalIgnoreCase));

            hasPatientProfileColumns =
                patientCols.Any(c => string.Equals(c, "FirstName", StringComparison.OrdinalIgnoreCase)) &&
                patientCols.Any(c => string.Equals(c, "LastName", StringComparison.OrdinalIgnoreCase)) &&
                patientCols.Any(c => string.Equals(c, "BirthDate", StringComparison.OrdinalIgnoreCase)) &&
                patientCols.Any(c => string.Equals(c, "Location", StringComparison.OrdinalIgnoreCase)) &&
                hasTransferStatusColumnPatients &&
                hasTargetTherapistIdColumnPatients;

            // ensure therapist invitations use Code (not legacy Token)
            if (!therapistInviteCols.Any(c => string.Equals(c, "Code", StringComparison.OrdinalIgnoreCase)))
                hasTherapistInvitations = false;
        }

        if (!hasTherapistInvitations ||
            !hasPatientInvitations ||
            !hasPatientActivities ||
            !hasAphasiaTypeColumnPatients ||
            !hasAphasiaTypeColumnInvites ||
            !hasTherapistProfileColumns ||
            !hasPatientProfileColumns)
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
    }
    catch
    {
        // Eğer sqlite_master sorgusu bile çalışmıyorsa, güvenli tarafta kalıp yeniden oluştur.
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }
}

// Swagger UI: /swagger
app.UseSwagger();
app.UseSwaggerUI();

// API endpoints
app.MapApi();

app.Run();
