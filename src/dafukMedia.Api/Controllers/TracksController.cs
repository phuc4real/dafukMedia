using Microsoft.AspNetCore.Mvc;
using dafukMedia.DTO;
using dafukMedia.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using dafukMedia.Data.DBContext;

namespace dafukMedia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TracksController : ControllerBase
{
    private readonly dafukMediaContext _context;

    public TracksController(dafukMediaContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all tracks with basic information
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TrackDto>>>> GetTracks(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        [FromQuery] string? artist = null,
        [FromQuery] string? album = null,
        [FromQuery] string? genre = null)
    {
        try
        {
            var query = _context.Tracks
                .Include(t => t.Artist)
                .Include(t => t.Album)
                .Include(t => t.Genre)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t => t.Title.Contains(search) ||
                                   (t.Artist != null && t.Artist.Name.Contains(search)) ||
                                   (t.Album != null && t.Album.Title.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(artist))
            {
                query = query.Where(t => t.Artist != null && t.Artist.Name.Contains(artist));
            }

            if (!string.IsNullOrWhiteSpace(album))
            {
                query = query.Where(t => t.Album != null && t.Album.Title.Contains(album));
            }

            if (!string.IsNullOrWhiteSpace(genre))
            {
                query = query.Where(t => t.Genre != null && t.Genre.Name.Contains(genre));
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var tracks = await query
                .OrderBy(t => t.Artist!.Name)
                .ThenBy(t => t.Album!.Title)
                .ThenBy(t => t.TrackNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TrackDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    FilePath = t.FilePath,
                    FileFormat = t.FileFormat,
                    FileSize = t.FileSize,
                    TrackNumber = t.TrackNumber,
                    DiscNumber = t.DiscNumber,
                    Duration = t.Duration,
                    Year = t.Year,
                    BitRate = t.BitRate,
                    SampleRate = t.SampleRate,
                    ArtistName = t.Artist != null ? t.Artist.Name : null,
                    AlbumTitle = t.Album != null ? t.Album.Title : null,
                    GenreName = t.Genre != null ? t.Genre.Name : null,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .ToListAsync();

            var response = new
            {
                Tracks = tracks,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<TrackDto>>.ErrorResult($"Error retrieving tracks: {ex.Message}"));
        }
    }

    /// <summary>
    /// Get a specific track by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TrackDto>>> GetTrack(int id)
    {
        try
        {
            var track = await _context.Tracks
                .Include(t => t.Artist)
                .Include(t => t.Album)
                .Include(t => t.Genre)
                .Where(t => t.Id == id)
                .Select(t => new TrackDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    FilePath = t.FilePath,
                    FileFormat = t.FileFormat,
                    FileSize = t.FileSize,
                    TrackNumber = t.TrackNumber,
                    DiscNumber = t.DiscNumber,
                    Duration = t.Duration,
                    Year = t.Year,
                    BitRate = t.BitRate,
                    SampleRate = t.SampleRate,
                    ArtistName = t.Artist != null ? t.Artist.Name : null,
                    AlbumTitle = t.Album != null ? t.Album.Title : null,
                    GenreName = t.Genre != null ? t.Genre.Name : null,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (track == null)
                return NotFound(ApiResponse<TrackDto>.ErrorResult("Track not found"));

            return Ok(ApiResponse<TrackDto>.SuccessResult(track));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<TrackDto>.ErrorResult($"Error retrieving track: {ex.Message}"));
        }
    }

    /// <summary>
    /// Get all artists for metadata editing
    /// </summary>
    [HttpGet("artists")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetArtists()
    {
        try
        {
            var artists = await _context.Artists
                .OrderBy(a => a.Name)
                .Select(a => a.Name)
                .ToListAsync();

            return Ok(ApiResponse<List<string>>.SuccessResult(artists));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<string>>.ErrorResult($"Error retrieving artists: {ex.Message}"));
        }
    }

    /// <summary>
    /// Get all albums for metadata editing
    /// </summary>
    [HttpGet("albums")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetAlbums()
    {
        try
        {
            var albums = await _context.Albums
                .OrderBy(a => a.Title)
                .Select(a => a.Title)
                .ToListAsync();

            return Ok(ApiResponse<List<string>>.SuccessResult(albums));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<string>>.ErrorResult($"Error retrieving albums: {ex.Message}"));
        }
    }

    /// <summary>
    /// Get all genres for metadata editing
    /// </summary>
    [HttpGet("genres")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetGenres()
    {
        try
        {
            var genres = await _context.Genres
                .OrderBy(g => g.Name)
                .Select(g => g.Name)
                .ToListAsync();

            return Ok(ApiResponse<List<string>>.SuccessResult(genres));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<string>>.ErrorResult($"Error retrieving genres: {ex.Message}"));
        }
    }

    /// <summary>
    /// Search tracks by multiple criteria for metadata editing
    /// </summary>
    [HttpPost("search")]
    public async Task<ActionResult<ApiResponse<List<TrackDto>>>> SearchTracks([FromBody] TrackSearchDto searchCriteria)
    {
        try
        {
            var query = _context.Tracks
                .Include(t => t.Artist)
                .Include(t => t.Album)
                .Include(t => t.Genre)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchCriteria.Title))
                query = query.Where(t => t.Title.Contains(searchCriteria.Title));

            if (!string.IsNullOrWhiteSpace(searchCriteria.Artist))
                query = query.Where(t => t.Artist != null && t.Artist.Name.Contains(searchCriteria.Artist));

            if (!string.IsNullOrWhiteSpace(searchCriteria.Album))
                query = query.Where(t => t.Album != null && t.Album.Title.Contains(searchCriteria.Album));

            if (!string.IsNullOrWhiteSpace(searchCriteria.Genre))
                query = query.Where(t => t.Genre != null && t.Genre.Name.Contains(searchCriteria.Genre));

            if (searchCriteria.Year.HasValue)
                query = query.Where(t => t.Year == searchCriteria.Year);

            if (searchCriteria.HasMissingMetadata.HasValue && searchCriteria.HasMissingMetadata.Value)
            {
                query = query.Where(t => t.Artist == null || t.Album == null || t.Genre == null || 
                                       string.IsNullOrEmpty(t.Title) || !t.Year.HasValue);
            }

            var tracks = await query
                .OrderBy(t => t.Artist!.Name)
                .ThenBy(t => t.Album!.Title)
                .ThenBy(t => t.TrackNumber)
                .Take(searchCriteria.MaxResults ?? 100)
                .Select(t => new TrackDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    FilePath = t.FilePath,
                    FileFormat = t.FileFormat,
                    FileSize = t.FileSize,
                    TrackNumber = t.TrackNumber,
                    DiscNumber = t.DiscNumber,
                    Duration = t.Duration,
                    Year = t.Year,
                    BitRate = t.BitRate,
                    SampleRate = t.SampleRate,
                    ArtistName = t.Artist != null ? t.Artist.Name : null,
                    AlbumTitle = t.Album != null ? t.Album.Title : null,
                    GenreName = t.Genre != null ? t.Genre.Name : null,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .ToListAsync();

            return Ok(ApiResponse<List<TrackDto>>.SuccessResult(tracks));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<TrackDto>>.ErrorResult($"Error searching tracks: {ex.Message}"));
        }
    }
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