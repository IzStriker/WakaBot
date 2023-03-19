﻿using Microsoft.EntityFrameworkCore;
using WakaBot.Core.Models;

namespace WakaBot.Core.Data
#pragma warning disable CS8618
{
    /// <summary>
    /// Context for WakaBot's database.
    /// </summary>
    public class WakaContext : DbContext
    {

        public DbSet<User> Users { get; set; }
        public DbSet<DiscordUser> DiscordUsers { get; set; }
        public DbSet<DiscordGuild> DiscordGuilds { get; set; }
        public DbSet<WakaUser> WakaUsers { get; set; }
        protected IConfigurationRoot Configuration;

        /// <summary>
        /// Create instance of database context.
        /// </summary>
        /// <param name="opt">Database configurations options.</param>
        public WakaContext(DbContextOptions<WakaContext> opt)
            : base(opt)
        {
            GetConfiguration();
        }

        public WakaContext()
        {
            GetConfiguration();
        }


        private void GetConfiguration()
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables("DOTNET_")
            .Build();
            Configuration = configuration;
        }

    }
}
#pragma warning restore CS8618
