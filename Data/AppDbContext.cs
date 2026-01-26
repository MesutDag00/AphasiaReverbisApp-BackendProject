using Microsoft.EntityFrameworkCore;
using AphaisaReverbes.Models;

namespace AphaisaReverbes.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Therapist> Therapists => Set<Therapist>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<TherapistInvitation> TherapistInvitations => Set<TherapistInvitation>();
    public DbSet<PatientInvitation> PatientInvitations => Set<PatientInvitation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Therapist>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            b.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            b.Property(x => x.GraduationDate).IsRequired();
            b.Property(x => x.BirthDate).IsRequired();
            b.Property(x => x.Location).HasMaxLength(200).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.HasIndex(x => new { x.LastName, x.FirstName });
        });

        modelBuilder.Entity<Patient>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            b.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            b.Property(x => x.BirthDate).IsRequired();
            b.Property(x => x.Location).HasMaxLength(200).IsRequired();
            b.Property(x => x.AphasiaType)
                .HasConversion<string>()
                .HasMaxLength(64)
                .IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();

            b.HasOne(x => x.Therapist)
                .WithMany(x => x.Patients)
                .HasForeignKey(x => x.TherapistId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(x => new { x.LastName, x.FirstName });
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

