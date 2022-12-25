﻿using Microsoft.EntityFrameworkCore;
using WakaBot.Core.Models;

namespace WakaBot.Core.Data
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
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables("DOTNET_")
                .Build();
            opt.UseMySql(configuration.GetConnectionString("MySql"), new MySqlServerVersion(new Version(5, 7)));
            base.OnConfiguring(opt);
        }

    }
}