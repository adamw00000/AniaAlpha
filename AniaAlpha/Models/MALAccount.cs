using Discord;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AniaAlpha.Models
{
    class MALAccount
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public ulong UserId { get; set; }
        [Required]
        public ulong GuildId { get; set; }
        [Required]
        public string UserName { get; set; }
    }
}
