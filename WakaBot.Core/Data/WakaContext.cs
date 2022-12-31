using Microsoft.EntityFrameworkCore;
using WakaBot.Core.Models;

namespace WakaBot.Core.Data
{
    /// <summary>
    /// Context for WakaBot's database.
    /// </summary>
    public class WakaContext : DbContext
    {

#pragma warning disable CS8618
        public DbSet<User> Users { get; set; }
        public DbSet<DiscordUser> DiscordUsers { get; set; }
        public DbSet<DiscordGuild> DiscordGuilds { get; set; }
        public DbSet<WakaUser> WakaUsers { get; set; }

        /// <summary>
        /// Create instance of database context.
        /// </summary>
        /// <param name="opt">Database configurations options.</param>
        public WakaContext(DbContextOptions<WakaContext> opt)
            : base(opt)
        { }

        public WakaContext() { }
#pragma warning restore CS8618

        protected override void OnConfiguring(DbContextOptionsBuilder opt)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables("DOTNET_")
                .Build();
            opt.UseMySql(configuration.GetConnectionString("MySql"), new MySqlServerVersion(new Version(5, 7)));
            base.OnConfiguring(opt);
        }

    }
}
