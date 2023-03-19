using Microsoft.EntityFrameworkCore;

namespace WakaBot.Core.Data;

public class MySqlContext : WakaContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder opt)
    {
        opt.UseMySql(Configuration.GetConnectionString("MySql"), new MySqlServerVersion(new Version(5, 7)));
        base.OnConfiguring(opt);
    }
}