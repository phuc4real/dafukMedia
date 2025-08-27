using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dafukMedia.Data.Models
{
    public class Album
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? SortTitle { get; set; }

        public int? Year { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public string? AlbumArtPath { get; set; }

        public int? ArtistId { get; set; }
        public int? GenreId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ArtistId")]
        public virtual Artist? Artist { get; set; }

        [ForeignKey("GenreId")]
        public virtual Genre? Genre { get; set; }

        public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();
    }
}
