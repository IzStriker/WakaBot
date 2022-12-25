using System.ComponentModel.DataAnnotations;

namespace WakaBot.Core.Models
{
    /// <summary>
    /// Database representation of user registered to WakaBot.
    /// </summary>
    public class User
    {
        public int Id { get; set; }

        [Required]
        public ulong DiscordId { get; set; }

        [Required]
        public string WakaName { get; set; } = string.Empty;

        [Required]
        public ulong GuildId { get; set; }
    }
}
