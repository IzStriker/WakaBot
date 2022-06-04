using Microsoft.EntityFrameworkCore;
using WakaBot.Models;

namespace WakaBot.Data
{
    public class WakaContext : DbContext
    {

        public DbSet<User> Users { get; set; }

        public WakaContext(DbContextOptions<WakaContext> opt)
            : base(opt)
        { }

    }
}
