using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WakaBot.Core.Data;

/// <summary>
/// Used by Entity Framework when updating migrations.
/// https://stackoverflow.com/a/60602620/16322117
/// </summary>
public class WakaContextFactory : IDesignTimeDbContextFactory<WakaContext>
{
    /// <summary>
    /// Create instance of WakaTime DBContext
    /// </summary>
    /// <returns>Instance of DBContext</returns>
    public WakaContext CreateDbContext(string[] args) => new WakaContext();

}