namespace dafukMedia.DTO;

public class ScanJobDto
{
    public int Id { get; set; }
    public string DirectoryPath { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int FilesScanned { get; set; }
    public int FilesAdded { get; set; }
    public int DuplicatesFound { get; set; }
    public int ErrorsCount { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan? Duration => EndTime?.Subtract(StartTime);
}

public class ScanJobCreateDto
{
    public string DirectoryPath { get; set; } = string.Empty;
}

public class ScanProgressDto
{
    public int JobId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int FilesScanned { get; set; }
    public int FilesAdded { get; set; }
    public int DuplicatesFound { get; set; }
    public int ErrorsCount { get; set; }
    public string? CurrentFile { get; set; }
    public double ProgressPercentage { get; set; }
}