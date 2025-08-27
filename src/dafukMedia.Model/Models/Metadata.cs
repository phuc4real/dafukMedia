using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dafukMedia.Data.Models
{
    public class Metadata
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TrackId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty; // e.g., "COMMENT", "LYRICS", "COMPOSER"

        public string? Value { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("TrackId")]
        public virtual Track Track { get; set; } = null!;
    }
}
