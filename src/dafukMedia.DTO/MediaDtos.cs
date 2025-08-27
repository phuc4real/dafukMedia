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