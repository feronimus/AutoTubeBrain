using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure;

public sealed class YtbDbContextFactory : IDesignTimeDbContextFactory<YtbDbContext>
{
    public YtbDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__Main")
                 ?? "Host=localhost;Port=5432;Database=ytb;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<YtbDbContext>()
            .UseNpgsql(cs)
            .Options;

        return new YtbDbContext(options);
    }
}
