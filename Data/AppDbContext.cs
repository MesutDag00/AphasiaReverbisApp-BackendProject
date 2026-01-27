using Microsoft.EntityFrameworkCore;
using AphaisaReverbes.Models;

namespace AphaisaReverbes.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Therapist> Therapists => Set<Therapist>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<PatientActivity> PatientActivities => Set<PatientActivity>();
    public DbSet<TherapistInvitation> TherapistInvitations => Set<TherapistInvitation>();
    public DbSet<PatientInvitation> PatientInvitations => Set<PatientInvitation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<City>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Therapist>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            b.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            b.Property(x => x.Email).HasMaxLength(320).IsRequired();
            b.Property(x => x.PasswordHash).HasMaxLength(200).IsRequired();
            b.Property(x => x.GraduationDate).IsRequired();
            b.Property(x => x.BirthDate).IsRequired();
            b.Property(x => x.Gender)
                .HasConversion<string>()
                .HasMaxLength(16)
                .IsRequired();
            b.Property(x => x.PhoneNumber).HasMaxLength(32);
            b.HasOne(x => x.City)
                .WithMany()
                .HasForeignKey(x => x.CityId)
                .OnDelete(DeleteBehavior.SetNull);
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.HasIndex(x => x.Email).IsUnique();
            b.HasIndex(x => new { x.LastName, x.FirstName });
        });

        modelBuilder.Entity<Patient>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            b.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            b.Property(x => x.Email).HasMaxLength(320).IsRequired();
            b.Property(x => x.PasswordHash).HasMaxLength(200).IsRequired();
            b.Property(x => x.BirthDate).IsRequired();
            b.Property(x => x.Gender)
                .HasConversion<string>()
                .HasMaxLength(16)
                .IsRequired();
            b.Property(x => x.PhoneNumber).HasMaxLength(32);
            b.HasOne(x => x.City)
                .WithMany()
                .HasForeignKey(x => x.CityId)
                .OnDelete(DeleteBehavior.SetNull);
            b.Property(x => x.AphasiaType)
                .HasConversion<string>()
                .HasMaxLength(64)
                .IsRequired();
            b.Property(x => x.TransferStatus)
                .HasConversion<string>()
                .HasMaxLength(16)
                .IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();

            b.HasOne(x => x.Therapist)
                .WithMany(x => x.Patients)
                .HasForeignKey(x => x.TherapistId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(x => x.Email).IsUnique();
            b.HasIndex(x => new { x.LastName, x.FirstName });
            b.HasIndex(x => new { x.TargetTherapistId, x.TransferStatus });
        });

        modelBuilder.Entity<PatientActivity>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.ActivityName).HasMaxLength(200).IsRequired();
            b.Property(x => x.Score).IsRequired();
            b.Property(x => x.Duration).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();

            b.HasOne(x => x.Patient)
                .WithMany(p => p.Activities)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.PatientId);
            b.HasIndex(x => x.CreatedAt);
        });

        modelBuilder.Entity<TherapistInvitation>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Code).HasMaxLength(6).IsRequired();
            b.HasIndex(x => x.Code).IsUnique();
            b.Property(x => x.CreatedAtUtc).IsRequired();
        });

        modelBuilder.Entity<PatientInvitation>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Code).HasMaxLength(6).IsRequired();
            b.Property(x => x.AphasiaType)
                .HasConversion<string>()
                .HasMaxLength(64)
                .IsRequired();
            b.HasIndex(x => x.Code).IsUnique();
            b.Property(x => x.CreatedAtUtc).IsRequired();

            b.HasOne(x => x.Therapist)
                .WithMany()
                .HasForeignKey(x => x.TherapistId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.TherapistId);
        });
    }
}

