namespace dafukMedia.DTO;

public class TrackDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileFormat { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int? TrackNumber { get; set; }
    public int? DiscNumber { get; set; }
    public TimeSpan? Duration { get; set; }
    public int? Year { get; set; }
    public int? BitRate { get; set; }
    public int? SampleRate { get; set; }
    public string? ArtistName { get; set; }
    public string? AlbumTitle { get; set; }
    public string? GenreName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TrackMetadataDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public string? Genre { get; set; }
    public int? Year { get; set; }
    public int? TrackNumber { get; set; }
    public int? DiscNumber { get; set; }
    public string? Comment { get; set; }
    public string? Composer { get; set; }
    public string? AlbumArtist { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public Dictionary<string, string> AdditionalMetadata { get; set; } = new();
}

public class EditTrackMetadataDto
{
    public string Title { get; set; } = string.Empty;
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public string? Genre { get; set; }
    public int? Year { get; set; }
    public int? TrackNumber { get; set; }
    public int? DiscNumber { get; set; }
    public string? Comment { get; set; }
    public string? Composer { get; set; }
    public string? AlbumArtist { get; set; }
    public Dictionary<string, string> AdditionalMetadata { get; set; } = new();
}

public class BatchEditMetadataDto
{
    public List<int> TrackIds { get; set; } = new();
    public EditTrackMetadataDto Metadata { get; set; } = new();
    public bool OverwriteExisting { get; set; } = false;
}

public class MetadataValidationResultDto
{
    public int TrackId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public List<string> MissingFields { get; set; } = new();
    public List<string> InvalidFields { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class MetadataEditResultDto
{
    public int TrackId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> UpdatedFields { get; set; } = new();
}

public class TrackSearchDto
{
    public string? Title { get; set; }
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public string? Genre { get; set; }
    public int? Year { get; set; }
    public bool? HasMissingMetadata { get; set; }
    public int? MaxResults { get; set; } = 100;
}

public class ArtistDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SortName { get; set; }
    public string? Biography { get; set; }
    public string? ImagePath { get; set; }
    public int AlbumCount { get; set; }
    public int TrackCount { get; set; }
}

public class AlbumDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? SortTitle { get; set; }
    public int? Year { get; set; }
    public string? Description { get; set; }
    public string? AlbumArtPath { get; set; }
    public string? ArtistName { get; set; }
    public string? GenreName { get; set; }
    public int TrackCount { get; set; }
}

public class GenreDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TrackCount { get; set; }
    public int AlbumCount { get; set; }
}