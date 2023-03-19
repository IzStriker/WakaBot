using Microsoft.EntityFrameworkCore;

namespace WakaBot.Core.Data;

public class SqliteContext : WakaContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder opt)
    {
        opt.UseSqlite(Configuration.GetConnectionString("Sqlite"));
        base.OnConfiguring(opt);
    }

}