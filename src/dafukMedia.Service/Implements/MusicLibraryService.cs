using dafukMedia.Data.DBContext;
using dafukMedia.DTO;
using dafukMedia.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace dafukMedia.Service.Implements;

public class MusicLibraryService : IMusicLibraryService
{
    private readonly dafukMediaContext _context;

    public MusicLibraryService(dafukMediaContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PagedResponse<TrackDto>>> GetTracksAsync(int pageNumber = 1, int pageSize = 50, string? searchTerm = null)
    {
        var query = _context.Tracks
            .Include(t => t.Artist)
            .Include(t => t.Album)
            .Include(t => t.Genre)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(t => 
                t.Title.Contains(searchTerm) ||
                (t.Artist != null && t.Artist.Name.Contains(searchTerm)) ||
                (t.Album != null && t.Album.Title.Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync();
        var tracks = await query
            .OrderBy(t => t.Artist != null ? t.Artist.Name : "")
            .ThenBy(t => t.Album != null ? t.Album.Title : "")
            .ThenBy(t => t.TrackNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var trackDtos = tracks.Select(MapToTrackDto).ToList();

        var response = new PagedResponse<TrackDto>
        {
            Data = trackDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return ApiResponse<PagedResponse<TrackDto>>.SuccessResult(response);
    }

    public async Task<ApiResponse<PagedResponse<ArtistDto>>> GetArtistsAsync(int pageNumber = 1, int pageSize = 50, string? searchTerm = null)
    {
        var query = _context.Artists.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(a => a.Name.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync();
        var artists = await query
            .OrderBy(a => a.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ArtistDto
            {
                Id = a.Id,
                Name = a.Name,
                SortName = a.SortName,
                Biography = a.Biography,
                ImagePath = a.ImagePath,
                AlbumCount = a.Albums.Count,
                TrackCount = a.Tracks.Count
            })
            .ToListAsync();

        var response = new PagedResponse<ArtistDto>
        {
            Data = artists,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return ApiResponse<PagedResponse<ArtistDto>>.SuccessResult(response);
    }

    public async Task<ApiResponse<PagedResponse<AlbumDto>>> GetAlbumsAsync(int pageNumber = 1, int pageSize = 50, string? searchTerm = null)
    {
        var query = _context.Albums
            .Include(a => a.Artist)
            .Include(a => a.Genre)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(a => 
                a.Title.Contains(searchTerm) ||
                (a.Artist != null && a.Artist.Name.Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync();
        var albums = await query
            .OrderBy(a => a.Artist != null ? a.Artist.Name : "")
            .ThenBy(a => a.Year)
            .ThenBy(a => a.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AlbumDto
            {
                Id = a.Id,
                Title = a.Title,
                SortTitle = a.SortTitle,
                Year = a.Year,
                Description = a.Description,
                AlbumArtPath = a.AlbumArtPath,
                ArtistName = a.Artist != null ? a.Artist.Name : null,
                GenreName = a.Genre != null ? a.Genre.Name : null,
                TrackCount = a.Tracks.Count
            })
            .ToListAsync();

        var response = new PagedResponse<AlbumDto>
        {
            Data = albums,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return ApiResponse<PagedResponse<AlbumDto>>.SuccessResult(response);
    }

    public async Task<ApiResponse<PagedResponse<GenreDto>>> GetGenresAsync(int pageNumber = 1, int pageSize = 50, string? searchTerm = null)
    {
        var query = _context.Genres.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(g => g.Name.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync();
        var genres = await query
            .OrderBy(g => g.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(g => new GenreDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                TrackCount = 0, //TODO: Implement TrackCount if needed
                AlbumCount = 0 //TODO: Implement AlbumCount if needed
            })
            .ToListAsync();

        var response = new PagedResponse<GenreDto>
        {
            Data = genres,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return ApiResponse<PagedResponse<GenreDto>>.SuccessResult(response);
    }

    public async Task<ApiResponse<TrackDto>> GetTrackAsync(int id)
    {
        var track = await _context.Tracks
            .Include(t => t.Artist)
            .Include(t => t.Album)
            .Include(t => t.Genre)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (track == null)
        {
            return ApiResponse<TrackDto>.ErrorResult("Track not found");
        }

        var dto = MapToTrackDto(track);
        return ApiResponse<TrackDto>.SuccessResult(dto);
    }

    public async Task<ApiResponse<ArtistDto>> GetArtistAsync(int id)
    {
        var artist = await _context.Artists
            .Include(a => a.Albums)
            .Include(a => a.Tracks)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (artist == null)
        {
            return ApiResponse<ArtistDto>.ErrorResult("Artist not found");
        }

        var dto = new ArtistDto
        {
            Id = artist.Id,
            Name = artist.Name,
            SortName = artist.SortName,
            Biography = artist.Biography,
            ImagePath = artist.ImagePath,
            AlbumCount = artist.Albums.Count,
            TrackCount = artist.Tracks.Count
        };

        return ApiResponse<ArtistDto>.SuccessResult(dto);
    }

    public async Task<ApiResponse<AlbumDto>> GetAlbumAsync(int id)
    {
        var album = await _context.Albums
            .Include(a => a.Artist)
            .Include(a => a.Genre)
            .Include(a => a.Tracks)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (album == null)
        {
            return ApiResponse<AlbumDto>.ErrorResult("Album not found");
        }

        var dto = new AlbumDto
        {
            Id = album.Id,
            Title = album.Title,
            SortTitle = album.SortTitle,
            Year = album.Year,
            Description = album.Description,
            AlbumArtPath = album.AlbumArtPath,
            ArtistName = album.Artist?.Name,
            GenreName = album.Genre?.Name,
            TrackCount = album.Tracks.Count
        };

        return ApiResponse<AlbumDto>.SuccessResult(dto);
    }

    public async Task<ApiResponse<List<TrackDto>>> GetTracksByArtistAsync(int artistId)
    {
        var tracks = await _context.Tracks
            .Include(t => t.Artist)
            .Include(t => t.Album)
            .Include(t => t.Genre)
            .Where(t => t.ArtistId == artistId)
            .OrderBy(t => t.Album != null ? t.Album.Title : "")
            .ThenBy(t => t.TrackNumber)
            .ToListAsync();

        var dtos = tracks.Select(MapToTrackDto).ToList();
        return ApiResponse<List<TrackDto>>.SuccessResult(dtos);
    }

    public async Task<ApiResponse<List<TrackDto>>> GetTracksByAlbumAsync(int albumId)
    {
        var tracks = await _context.Tracks
            .Include(t => t.Artist)
            .Include(t => t.Album)
            .Include(t => t.Genre)
            .Where(t => t.AlbumId == albumId)
            .OrderBy(t => t.DiscNumber)
            .ThenBy(t => t.TrackNumber)
            .ToListAsync();

        var dtos = tracks.Select(MapToTrackDto).ToList();
        return ApiResponse<List<TrackDto>>.SuccessResult(dtos);
    }

    public async Task<ApiResponse<List<TrackDto>>> GetTracksByGenreAsync(int genreId)
    {
        var tracks = await _context.Tracks
            .Include(t => t.Artist)
            .Include(t => t.Album)
            .Include(t => t.Genre)
            .Where(t => t.GenreId == genreId)
            .OrderBy(t => t.Artist != null ? t.Artist.Name : "")
            .ThenBy(t => t.Album != null ? t.Album.Title : "")
            .ThenBy(t => t.TrackNumber)
            .ToListAsync();

        var dtos = tracks.Select(MapToTrackDto).ToList();
        return ApiResponse<List<TrackDto>>.SuccessResult(dtos);
    }

    public async Task<ApiResponse<List<TrackDto>>> GetUncategorizedTracksAsync()
    {
        var tracks = await _context.Tracks
            .Include(t => t.Artist)
            .Include(t => t.Album)
            .Include(t => t.Genre)
            .Where(t => t.ArtistId == null || t.AlbumId == null || t.GenreId == null)
            .OrderBy(t => t.Title)
            .ToListAsync();

        var dtos = tracks.Select(MapToTrackDto).ToList();
        return ApiResponse<List<TrackDto>>.SuccessResult(dtos);
    }

    public async Task<ApiResponse<List<TrackDto>>> GetTracksWithMissingMetadataAsync()
    {
        var tracks = await _context.Tracks
            .Include(t => t.Artist)
            .Include(t => t.Album)
            .Include(t => t.Genre)
            .Where(t => string.IsNullOrEmpty(t.Title) || 
                       t.TrackNumber == null || 
                       t.Duration == null)
            .OrderBy(t => t.Title)
            .ToListAsync();

        var dtos = tracks.Select(MapToTrackDto).ToList();
        return ApiResponse<List<TrackDto>>.SuccessResult(dtos);
    }

    private static TrackDto MapToTrackDto(dafukMedia.Data.Models.Track track)
    {
        return new TrackDto
        {
            Id = track.Id,
            Title = track.Title,
            FilePath = track.FilePath,
            FileFormat = track.FileFormat,
            FileSize = track.FileSize,
            TrackNumber = track.TrackNumber,
            DiscNumber = track.DiscNumber,
            Duration = track.Duration,
            Year = track.Year,
            BitRate = track.BitRate,
            SampleRate = track.SampleRate,
            ArtistName = track.Artist?.Name,
            AlbumTitle = track.Album?.Title,
            GenreName = track.Genre?.Name,
            CreatedAt = track.CreatedAt,
            UpdatedAt = track.UpdatedAt
        };
    }
}