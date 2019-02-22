using JikanDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AniaAlpha.Models
{
    public class AnimeAlias
    {
        [Key]
        public string Name { get; set; }
        [Required]
        public long AnimeId { get; set; }
    }
}
