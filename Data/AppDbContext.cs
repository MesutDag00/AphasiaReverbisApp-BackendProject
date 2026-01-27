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
            b.HasData(
                new City { Id = 1, Name = "Adana" },
                new City { Id = 2, Name = "Adıyaman" },
                new City { Id = 3, Name = "Afyonkarahisar" },
                new City { Id = 4, Name = "Ağrı" },
                new City { Id = 5, Name = "Amasya" },
                new City { Id = 6, Name = "Ankara" },
                new City { Id = 7, Name = "Antalya" },
                new City { Id = 8, Name = "Artvin" },
                new City { Id = 9, Name = "Aydın" },
                new City { Id = 10, Name = "Balıkesir" },
                new City { Id = 11, Name = "Bilecik" },
                new City { Id = 12, Name = "Bingöl" },
                new City { Id = 13, Name = "Bitlis" },
                new City { Id = 14, Name = "Bolu" },
                new City { Id = 15, Name = "Burdur" },
                new City { Id = 16, Name = "Bursa" },
                new City { Id = 17, Name = "Çanakkale" },
                new City { Id = 18, Name = "Çankırı" },
                new City { Id = 19, Name = "Çorum" },
                new City { Id = 20, Name = "Denizli" },
                new City { Id = 21, Name = "Diyarbakır" },
                new City { Id = 22, Name = "Edirne" },
                new City { Id = 23, Name = "Elazığ" },
                new City { Id = 24, Name = "Erzincan" },
                new City { Id = 25, Name = "Erzurum" },
                new City { Id = 26, Name = "Eskişehir" },
                new City { Id = 27, Name = "Gaziantep" },
                new City { Id = 28, Name = "Giresun" },
                new City { Id = 29, Name = "Gümüşhane" },
                new City { Id = 30, Name = "Hakkari" },
                new City { Id = 31, Name = "Hatay" },
                new City { Id = 32, Name = "Isparta" },
                new City { Id = 33, Name = "Mersin" },
                new City { Id = 34, Name = "İstanbul" },
                new City { Id = 35, Name = "İzmir" },
                new City { Id = 36, Name = "Kars" },
                new City { Id = 37, Name = "Kastamonu" },
                new City { Id = 38, Name = "Kayseri" },
                new City { Id = 39, Name = "Kırklareli" },
                new City { Id = 40, Name = "Kırşehir" },
                new City { Id = 41, Name = "Kocaeli" },
                new City { Id = 42, Name = "Konya" },
                new City { Id = 43, Name = "Kütahya" },
                new City { Id = 44, Name = "Malatya" },
                new City { Id = 45, Name = "Manisa" },
                new City { Id = 46, Name = "Kahramanmaraş" },
                new City { Id = 47, Name = "Mardin" },
                new City { Id = 48, Name = "Muğla" },
                new City { Id = 49, Name = "Muş" },
                new City { Id = 50, Name = "Nevşehir" },
                new City { Id = 51, Name = "Niğde" },
                new City { Id = 52, Name = "Ordu" },
                new City { Id = 53, Name = "Rize" },
                new City { Id = 54, Name = "Sakarya" },
                new City { Id = 55, Name = "Samsun" },
                new City { Id = 56, Name = "Siirt" },
                new City { Id = 57, Name = "Sinop" },
                new City { Id = 58, Name = "Sivas" },
                new City { Id = 59, Name = "Tekirdağ" },
                new City { Id = 60, Name = "Tokat" },
                new City { Id = 61, Name = "Trabzon" },
                new City { Id = 62, Name = "Tunceli" },
                new City { Id = 63, Name = "Şanlıurfa" },
                new City { Id = 64, Name = "Uşak" },
                new City { Id = 65, Name = "Van" },
                new City { Id = 66, Name = "Yozgat" },
                new City { Id = 67, Name = "Zonguldak" },
                new City { Id = 68, Name = "Aksaray" },
                new City { Id = 69, Name = "Bayburt" },
                new City { Id = 70, Name = "Karaman" },
                new City { Id = 71, Name = "Kırıkkale" },
                new City { Id = 72, Name = "Batman" },
                new City { Id = 73, Name = "Şırnak" },
                new City { Id = 74, Name = "Bartın" },
                new City { Id = 75, Name = "Ardahan" },
                new City { Id = 76, Name = "Iğdır" },
                new City { Id = 77, Name = "Yalova" },
                new City { Id = 78, Name = "Karabük" },
                new City { Id = 79, Name = "Kilis" },
                new City { Id = 80, Name = "Osmaniye" },
                new City { Id = 81, Name = "Düzce" }
            );
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
            b.Property(x => x.CityId).IsRequired();
            b.HasOne(x => x.City)
                .WithMany()
                .HasForeignKey(x => x.CityId)
                .OnDelete(DeleteBehavior.Restrict);
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
            b.Property(x => x.CityId).IsRequired();
            b.HasOne(x => x.City)
                .WithMany()
                .HasForeignKey(x => x.CityId)
                .OnDelete(DeleteBehavior.Restrict);
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

