using System.ComponentModel.DataAnnotations;

namespace WakaBot.Models
{

    public class User
    {
        public int Id { get; set; }

        [Required]
        public ulong DiscordId { get; set; }

        [Required]
        public string WakaName { get; set; } = string.Empty;

    }
}
