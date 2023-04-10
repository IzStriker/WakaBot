using Microsoft.EntityFrameworkCore;

namespace WakaBot.Core.Data;

public class MySqlContext : WakaContext
{
    public MySqlContext(DbContextOptions<WakaContext> options) : base(options) { }
}