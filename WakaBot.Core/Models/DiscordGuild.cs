using System.ComponentModel.DataAnnotations.Schema;

namespace WakaBot.Core.Models;

public class DiscordGuild
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public ulong Id { get; set; }
    public ICollection<DiscordUser> Users { get; set; } = new List<DiscordUser>();

}