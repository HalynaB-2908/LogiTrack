using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LogiTrack.WebApi.Data
{
    public class LogiTrackDbContextFactory : IDesignTimeDbContextFactory<LogiTrackDbContext>
    {
        public LogiTrackDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var cs = config.GetConnectionString("Postgres")
                     ?? throw new InvalidOperationException("ConnectionStrings:Postgres is not set.");

            var optionsBuilder = new DbContextOptionsBuilder<LogiTrackDbContext>();
            optionsBuilder.UseNpgsql(cs);

            return new LogiTrackDbContext(optionsBuilder.Options);
        }
    }
}
