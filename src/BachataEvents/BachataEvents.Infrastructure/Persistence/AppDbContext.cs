using BachataEvents.Domain.Entities;
using BachataEvents.Infrastructure.Auth;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BachataEvents.Infrastructure.Persistence;

public sealed class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<OrganizerProfile> OrganizerProfiles => Set<OrganizerProfile>();
    public DbSet<FestivalEvent> FestivalEvents => Set<FestivalEvent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<OrganizerProfile>(b =>
        {
            b.ToTable("OrganizerProfiles");
            b.HasKey(x => x.Id);

            b.Property(x => x.DisplayName).IsRequired().HasMaxLength(120);
            b.Property(x => x.UserId).IsRequired();

            b.Property(x => x.Website).HasMaxLength(500);
            b.Property(x => x.Instagram).HasMaxLength(200);

            b.HasIndex(x => x.UserId).IsUnique();

            b.HasMany(x => x.Festivals)
                .WithOne(x => x.OrganizerProfile)
                .HasForeignKey(x => x.OrganizerProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<FestivalEvent>(b =>
        {
            b.ToTable("FestivalEvents");
            b.HasKey(x => x.Id);

            b.Property(x => x.Title).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(4000);

            b.Property(x => x.Country).IsRequired().HasMaxLength(100);
            b.Property(x => x.City).IsRequired().HasMaxLength(120);
            b.Property(x => x.VenueName).HasMaxLength(200);

            // DateOnly mapping to SQL 'date'
            b.Property(x => x.StartDate).HasColumnType("date");
            b.Property(x => x.EndDate).HasColumnType("date");

            b.Property(x => x.WebsiteUrl).HasMaxLength(1000);
            b.Property(x => x.TicketUrl).HasMaxLength(1000);

            b.Property(x => x.CreatedAt).IsRequired();

            b.HasIndex(x => new { x.Country, x.StartDate });
            b.HasIndex(x => x.StartDate);
            b.HasIndex(x => x.OrganizerProfileId);
        });
    }
}
