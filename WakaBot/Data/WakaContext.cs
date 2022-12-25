using Microsoft.EntityFrameworkCore;
using WakaBot.Models;

namespace WakaBot.Data
{
    /// <summary>
    /// Context for WakaBot's database.
    /// </summary>
    public class WakaContext : DbContext
    {

        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Create instance of database context.
        /// </summary>
        /// <param name="opt">Database configurations options.</param>
        public WakaContext(DbContextOptions<WakaContext> opt)
            : base(opt)
        { }

        public WakaContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder opt)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables("DOTNET_")
                .Build();
            opt.UseMySql(configuration.GetConnectionString("MySql"), new MySqlServerVersion(new Version(5, 7)));
            base.OnConfiguring(opt);
        }

    }
}
