using dafukMedia.DTO;

namespace dafukMedia.Service.Interfaces;

public interface IMusicLibraryService
{
    Task<ApiResponse<PagedResponse<TrackDto>>> GetTracksAsync(int pageNumber = 1, int pageSize = 50, string? searchTerm = null);
    Task<ApiResponse<PagedResponse<ArtistDto>>> GetArtistsAsync(int pageNumber = 1, int pageSize = 50, string? searchTerm = null);
    Task<ApiResponse<PagedResponse<AlbumDto>>> GetAlbumsAsync(int pageNumber = 1, int pageSize = 50, string? searchTerm = null);
    Task<ApiResponse<PagedResponse<GenreDto>>> GetGenresAsync(int pageNumber = 1, int pageSize = 50, string? searchTerm = null);
    Task<ApiResponse<TrackDto>> GetTrackAsync(int id);
    Task<ApiResponse<ArtistDto>> GetArtistAsync(int id);
    Task<ApiResponse<AlbumDto>> GetAlbumAsync(int id);
    Task<ApiResponse<List<TrackDto>>> GetTracksByArtistAsync(int artistId);
    Task<ApiResponse<List<TrackDto>>> GetTracksByAlbumAsync(int albumId);
    Task<ApiResponse<List<TrackDto>>> GetTracksByGenreAsync(int genreId);
    Task<ApiResponse<List<TrackDto>>> GetUncategorizedTracksAsync();
    Task<ApiResponse<List<TrackDto>>> GetTracksWithMissingMetadataAsync();
}