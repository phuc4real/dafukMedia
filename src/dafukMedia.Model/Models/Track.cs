using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dafukMedia.Data.Models
{
    public class Track
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? SortTitle { get; set; }

        [Required]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string FileFormat { get; set; } = string.Empty; // MP3, FLAC, etc.

        public long FileSize { get; set; } // Size in bytes

        [MaxLength(32)]
        public string? FileHash { get; set; } // For duplicate detection

        public int? TrackNumber { get; set; }
        public int? DiscNumber { get; set; }
        public TimeSpan? Duration { get; set; }
        public int? Year { get; set; }
        public int? BitRate { get; set; }
        public int? SampleRate { get; set; }

        public int? ArtistId { get; set; }
        public int? AlbumId { get; set; }
        public int? GenreId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ArtistId")]
        public virtual Artist? Artist { get; set; }

        [ForeignKey("AlbumId")]
        public virtual Album? Album { get; set; }

        [ForeignKey("GenreId")]
        public virtual Genre? Genre { get; set; }

        public virtual ICollection<Metadata> Metadata { get; set; } = new List<Metadata>();
    }
}
