
using Microsoft.EntityFrameworkCore;

namespace WakaBot.Core.Data;
public class SqliteContext : WakaContext
{
    public SqliteContext(DbContextOptions<WakaContext> options) : base(options) { }
}
