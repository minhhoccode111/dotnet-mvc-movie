using System.ComponentModel.DataAnnotations;

namespace GameManagementMvc.Models
{
    public class Game
    {
        public int Id { get; set; }

        [StringLength(64, MinimumLength = 2)]
        [Required]
        public string? Title { get; set; }

        [StringLength(2048)]
        [Required]
        public string? Body { get; set; }

        // this not required
        [StringLength(2048)]
        public string? Image { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [DataType(DataType.Date)]
        [Required]
        public DateTime ReleaseDate { get; set; }

        [Required]
        public Company? Company { get; set; }

        [Required]
        public ICollection<Genre>? Genres { get; set; }
    }
}