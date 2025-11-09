using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public sealed class YtbDbContext : DbContext
{
    public YtbDbContext(DbContextOptions<YtbDbContext> options) : base(options) { }

    public DbSet<Episode> Episodes => Set<Episode>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Episode>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.ChannelKey, x.Slug }).IsUnique();
            e.Property(x => x.ChannelKey).HasMaxLength(64).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(128).IsRequired();
            e.Property(x => x.Title).HasMaxLength(256).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            e.Property(x => x.SceneJson).HasColumnType("jsonb");
        });

        b.Entity<OutboxMessage>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Type).HasMaxLength(128).IsRequired();
            e.Property(x => x.Payload).HasColumnType("jsonb").IsRequired();
            e.HasIndex(x => new { x.Dispatched, x.Id });
        });
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var cs = Environment.GetEnvironmentVariable("ConnectionStrings__Main")
                     ?? "Host=localhost;Port=5432;Database=ytb;Username=postgres;Password=postgres";
            optionsBuilder.UseNpgsql(cs);
        }
    }

}
