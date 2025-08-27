using System.ComponentModel.DataAnnotations;

namespace dafukMedia.Data.Models;

public class ScanJob
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string DirectoryPath { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, Running, Completed, Failed

    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public int FilesScanned { get; set; } = 0;
    public int FilesAdded { get; set; } = 0;
    public int DuplicatesFound { get; set; } = 0;
    public int ErrorsCount { get; set; } = 0;

    public string? ErrorMessage { get; set; }
    public string? LogPath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}