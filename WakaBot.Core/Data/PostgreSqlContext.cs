
using Microsoft.EntityFrameworkCore;

namespace WakaBot.Core.Data;

public class PostgreSqlContext : WakaContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder opt)
    {
        opt.UseNpgsql(Configuration.GetConnectionString("PostgreSql"));
        base.OnConfiguring(opt);
    }
}