using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventRoute> EventRoutes => Set<EventRoute>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(x => x.UserId);

            entity.Property(x => x.FirstName)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.LastName)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Email)
                .HasMaxLength(255)
                .IsRequired();

            entity.HasIndex(x => x.Email)
                .IsUnique();

            entity.Property(x => x.PasswordHash)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.Role)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.PhoneNumber)
                .HasMaxLength(30);

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .IsRequired();

            entity.HasMany(x => x.EventsCreated)
                .WithOne(x => x.Organiser)
                .HasForeignKey(x => x.OrganiserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Enrolments)
                .WithOne(x => x.Participant)
                .HasForeignKey(x => x.ParticipantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Events");

            entity.HasKey(x => x.EventId);

            entity.Property(x => x.OrganiserId)
                .IsRequired();

            entity.Property(x => x.EventName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(1000);

            entity.Property(x => x.EventDate)
                .IsRequired();

            entity.Property(x => x.Venue)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.City)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Province)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.EventType)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Upcoming")
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .IsRequired();

            entity.HasOne(x => x.Organiser)
                .WithMany(x => x.EventsCreated)
                .HasForeignKey(x => x.OrganiserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Categories)
                .WithOne(x => x.Event)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Routes)
                .WithOne(x => x.Event)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EventRoute>(entity =>
        {
            entity.ToTable("EventRoutes");

            entity.HasKey(x => x.RouteId);

            entity.Property(x => x.RouteName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.StartLocation)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.FinishLocation)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.RouteDistanceKm)
                .HasPrecision(6, 2)
                .IsRequired();

            entity.Property(x => x.RouteDescription)
                .HasMaxLength(1000);

            entity.HasIndex(x => new { x.EventId, x.RouteName })
                .IsUnique();

            entity.HasOne(x => x.Event)
                .WithMany(x => x.Routes)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");

            entity.HasKey(x => x.CategoryId);

            entity.Property(x => x.EventId)
                .IsRequired();

            entity.Property(x => x.CategoryName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.DistanceKm)
                .HasPrecision(6, 2)
                .IsRequired();

            entity.Property(x => x.EntryFee)
                .HasPrecision(10, 2)
                .IsRequired();

            entity.Property(x => x.MaximumParticipants);

            entity.HasIndex(x => new { x.EventId, x.CategoryName })
                .IsUnique();

            entity.HasOne(x => x.Event)
                .WithMany(x => x.Categories)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Enrolments)
                .WithOne(x => x.Category)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Enrolment>(entity =>
        {
            entity.ToTable("Enrolments");

            entity.HasKey(x => x.EnrolmentId);

            entity.Property(x => x.ParticipantId)
                .IsRequired();

            entity.Property(x => x.CategoryId)
                .IsRequired();

            entity.Property(x => x.EnrolmentDate)
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .IsRequired();

            entity.Property(x => x.EmergencyContactName)
                .HasMaxLength(100);

            entity.Property(x => x.EmergencyContactPhone)
                .HasMaxLength(30);

            entity.Property(x => x.PaymentStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Pending")
                .IsRequired();

            entity.Property(x => x.EnrolmentStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Active")
                .IsRequired();

            entity.HasIndex(x => new { x.ParticipantId, x.CategoryId })
                .IsUnique();

            entity.HasOne(x => x.Participant)
                .WithMany(x => x.Enrolments)
                .HasForeignKey(x => x.ParticipantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Category)
                .WithMany(x => x.Enrolments)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Result)
                .WithOne(x => x.Enrolment)
                .HasForeignKey(x => x.Result)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.ToTable("Results");

            entity.HasKey(x => x.ResultId);

            entity.Property(x => x.EnrolmentId)
                .IsRequired();

            entity.Property(x => x.ResultStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Finished")
                .IsRequired();

            entity.Property(x => x.RecordedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .IsRequired();

            entity.Property(x => x.FinishPosition)
                .IsRequired(false);

            entity.Property(x => x.FinishTimeSeconds)
                .IsRequired(false);

            entity.HasIndex(x => x.EnrolmentId)
                .IsUnique();

            entity.HasOne(x => x.Enrolment)
                .WithOne(x => x.Result)
                .HasForeignKey(x => x.EnrolmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
