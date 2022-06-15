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

    }
}
