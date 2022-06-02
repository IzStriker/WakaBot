using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WakaBot.Models;

namespace WakaBot.Data
{
    public class WakaContext : DbContext
    {

        public DbSet<User> Users { get; set; }
        private IConfiguration _configuration;
        public WakaContext()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string path = Path.Join(
                _configuration["dBPath"] ?? AppContext.BaseDirectory,
                 _configuration["dBFileName"] ?? "waka.db");
            optionsBuilder.UseSqlite($"Data Source={path}");
        }
    }
}
