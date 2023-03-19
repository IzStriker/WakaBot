using Microsoft.EntityFrameworkCore;

namespace WakaBot.Core.Data;

public class PostgreSqlContext : WakaContext
{
    public PostgreSqlContext(DbContextOptions<WakaContext> options) : base(options) { }
}