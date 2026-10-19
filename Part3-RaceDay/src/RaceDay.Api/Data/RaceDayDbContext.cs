using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

/// <summary>
/// Entity Framework Core context for the RaceDay platform.
/// Uses SQLite so the API runs identically on a workstation and inside a container.
/// </summary>
public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<RaceResult> Results => Set<RaceResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---- User ----
        modelBuilder.Entity<User>(e =>
        {
            e.Property(u => u.Email).HasMaxLength(256).IsRequired();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasConversion<int>();
            e.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            e.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        });

        // ---- Event ----
        modelBuilder.Entity<Event>(e =>
        {
            e.Property(ev => ev.Name).HasMaxLength(150).IsRequired();
            e.Property(ev => ev.Location).HasMaxLength(150).IsRequired();
            e.Property(ev => ev.EventType).HasConversion<int>();
            e.Property(ev => ev.DistanceKm).HasPrecision(6, 2);
            e.HasOne(ev => ev.Organiser)
                .WithMany(u => u.OrganisedEvents)
                .HasForeignKey(ev => ev.OrganiserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- Category ----
        modelBuilder.Entity<Category>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(120).IsRequired();
            e.Property(c => c.DistanceKm).HasPrecision(6, 2);
            e.Property(c => c.EntryFee).HasPrecision(10, 2);
            e.HasOne(c => c.Event)
                .WithMany(ev => ev.Categories)
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- Enrolment ----
        modelBuilder.Entity<Enrolment>(e =>
        {
            e.Property(en => en.Status).HasConversion<int>();
            // A participant may only have one enrolment row per event.
            e.HasIndex(en => new { en.EventId, en.ParticipantId }).IsUnique();

            e.HasOne(en => en.Event)
                .WithMany(ev => ev.Enrolments)
                .HasForeignKey(en => en.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(en => en.Participant)
                .WithMany(u => u.Enrolments)
                .HasForeignKey(en => en.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(en => en.Category)
                .WithMany(c => c.Enrolments)
                .HasForeignKey(en => en.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ---- RaceResult ----
        modelBuilder.Entity<RaceResult>(e =>
        {
            e.HasOne(r => r.Enrolment)
                .WithOne(en => en.Result)
                .HasForeignKey<RaceResult>(r => r.EnrolmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
