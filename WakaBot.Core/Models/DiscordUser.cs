using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WakaBot.Core.Models;

[Index(nameof(WakaUserId), IsUnique = true)]
public class DiscordUser
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public ulong Id { get; set; }
    public ICollection<DiscordGuild> Guilds { get; set; } = new List<DiscordGuild>();
    public string? WakaUserId { get; set; }
    public WakaUser? WakaUser { get; set; }
}