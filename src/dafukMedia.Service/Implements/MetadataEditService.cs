using dafukMedia.Data.Models;
using dafukMedia.DTO;
using dafukMedia.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using dafukMedia.Data.DBContext;
using System.ComponentModel.DataAnnotations;

namespace dafukMedia.Service.Implements;

public class MetadataEditService : IMetadataEditService
{
    private readonly dafukMediaContext _context;
    private readonly IMetadataExtractionService _metadataExtractionService;

    public MetadataEditService(dafukMediaContext context, IMetadataExtractionService metadataExtractionService)
    {
        _context = context;
        _metadataExtractionService = metadataExtractionService;
    }

    public async Task<ApiResponse<TrackMetadataDto>> GetTrackMetadataAsync(int trackId)
    {
        try
        {
            var track = await _context.Tracks
                .Include(t => t.Artist)
                .Include(t => t.Album)
                .Include(t => t.Genre)
                .Include(t => t.Metadata)
                .FirstOrDefaultAsync(t => t.Id == trackId);

            if (track == null)
                return ApiResponse<TrackMetadataDto>.ErrorResult("Track not found");

            var additionalMetadata = track.Metadata.ToDictionary(m => m.Key, m => m.Value);

            var trackMetadata = new TrackMetadataDto
            {
                Id = track.Id,
                Title = track.Title,
                Artist = track.Artist?.Name,
                Album = track.Album?.Title,
                Genre = track.Genre?.Name,
                Year = track.Year,
                TrackNumber = track.TrackNumber,
                DiscNumber = track.DiscNumber,
                Comment = additionalMetadata.GetValueOrDefault("COMMENT"),
                Composer = additionalMetadata.GetValueOrDefault("COMPOSER"),
                AlbumArtist = additionalMetadata.GetValueOrDefault("ALBUMARTIST"),
                FilePath = track.FilePath,
                AdditionalMetadata = additionalMetadata
            };

            return ApiResponse<TrackMetadataDto>.SuccessResult(trackMetadata);
        }
        catch (Exception ex)
        {
            return ApiResponse<TrackMetadataDto>.ErrorResult($"Error retrieving track metadata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<MetadataEditResultDto>> EditTrackMetadataAsync(int trackId, EditTrackMetadataDto metadata)
    {
        try
        {
            var track = await _context.Tracks
                .Include(t => t.Artist)
                .Include(t => t.Album)
                .Include(t => t.Genre)
                .Include(t => t.Metadata)
                .FirstOrDefaultAsync(t => t.Id == trackId);

            if (track == null)
                return ApiResponse<MetadataEditResultDto>.ErrorResult("Track not found");

            if (!File.Exists(track.FilePath))
                return ApiResponse<MetadataEditResultDto>.ErrorResult("Audio file not found");

            var validationResult = ValidateMetadata(metadata);
            if (validationResult.Any())
            {
                return ApiResponse<MetadataEditResultDto>.ErrorResult($"Validation errors: {string.Join(", ", validationResult)}");
            }

            var result = new MetadataEditResultDto
            {
                TrackId = trackId,
                FilePath = track.FilePath,
                Success = false,
                UpdatedFields = new List<string>()
            };

            try
            {
                // Update the audio file tags using TagLibSharp
                using var file = TagLib.File.Create(track.FilePath);
                
                if (!string.IsNullOrWhiteSpace(metadata.Title) && metadata.Title != track.Title)
                {
                    file.Tag.Title = metadata.Title;
                    track.Title = metadata.Title;
                    result.UpdatedFields.Add("Title");
                }

                if (!string.IsNullOrWhiteSpace(metadata.Artist))
                {
                    file.Tag.Performers = new[] { metadata.Artist };
                    track.Artist = await GetOrCreateArtistAsync(metadata.Artist);
                    result.UpdatedFields.Add("Artist");
                }

                if (!string.IsNullOrWhiteSpace(metadata.Album))
                {
                    file.Tag.Album = metadata.Album;
                    track.Album = await GetOrCreateAlbumAsync(metadata.Album, metadata.Year);
                    result.UpdatedFields.Add("Album");
                }

                if (!string.IsNullOrWhiteSpace(metadata.Genre))
                {
                    file.Tag.Genres = new[] { metadata.Genre };
                    track.Genre = await GetOrCreateGenreAsync(metadata.Genre);
                    result.UpdatedFields.Add("Genre");
                }

                if (metadata.Year.HasValue && metadata.Year != track.Year)
                {
                    file.Tag.Year = (uint)metadata.Year.Value;
                    track.Year = metadata.Year;
                    result.UpdatedFields.Add("Year");
                }

                if (metadata.TrackNumber.HasValue && metadata.TrackNumber != track.TrackNumber)
                {
                    file.Tag.Track = (uint)metadata.TrackNumber.Value;
                    track.TrackNumber = metadata.TrackNumber;
                    result.UpdatedFields.Add("TrackNumber");
                }

                if (metadata.DiscNumber.HasValue && metadata.DiscNumber != track.DiscNumber)
                {
                    file.Tag.Disc = (uint)metadata.DiscNumber.Value;
                    track.DiscNumber = metadata.DiscNumber;
                    result.UpdatedFields.Add("DiscNumber");
                }

                if (!string.IsNullOrWhiteSpace(metadata.Comment))
                {
                    file.Tag.Comment = metadata.Comment;
                    await UpdateAdditionalMetadataAsync(track, "COMMENT", metadata.Comment);
                    result.UpdatedFields.Add("Comment");
                }

                if (!string.IsNullOrWhiteSpace(metadata.Composer))
                {
                    file.Tag.Composers = new[] { metadata.Composer };
                    await UpdateAdditionalMetadataAsync(track, "COMPOSER", metadata.Composer);
                    result.UpdatedFields.Add("Composer");
                }

                if (!string.IsNullOrWhiteSpace(metadata.AlbumArtist))
                {
                    file.Tag.AlbumArtists = new[] { metadata.AlbumArtist };
                    await UpdateAdditionalMetadataAsync(track, "ALBUMARTIST", metadata.AlbumArtist);
                    result.UpdatedFields.Add("AlbumArtist");
                }

                // Handle additional metadata
                foreach (var kvp in metadata.AdditionalMetadata)
                {
                    if (!string.IsNullOrWhiteSpace(kvp.Value))
                    {
                        await UpdateAdditionalMetadataAsync(track, kvp.Key, kvp.Value);
                        result.UpdatedFields.Add(kvp.Key);
                    }
                }

                // Save changes to the audio file
                file.Save();

                // Update the database
                track.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Error updating file tags: {ex.Message}";
            }

            return ApiResponse<MetadataEditResultDto>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<MetadataEditResultDto>.ErrorResult($"Error editing track metadata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<MetadataEditResultDto>>> BatchEditMetadataAsync(BatchEditMetadataDto batchRequest)
    {
        var results = new List<MetadataEditResultDto>();

        try
        {
            foreach (var trackId in batchRequest.TrackIds)
            {
                var editResult = await EditTrackMetadataAsync(trackId, batchRequest.Metadata);
                if (editResult.Success && editResult.Data != null)
                {
                    results.Add(editResult.Data);
                }
                else
                {
                    results.Add(new MetadataEditResultDto
                    {
                        TrackId = trackId,
                        Success = false,
                        ErrorMessage = editResult.Message
                    });
                }
            }

            return ApiResponse<List<MetadataEditResultDto>>.SuccessResult(results);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<MetadataEditResultDto>>.ErrorResult($"Error during batch edit: {ex.Message}");
        }
    }

    public async Task<ApiResponse<MetadataValidationResultDto>> ValidateTrackMetadataAsync(int trackId)
    {
        try
        {
            var track = await _context.Tracks
                .Include(t => t.Artist)
                .Include(t => t.Album)
                .Include(t => t.Genre)
                .FirstOrDefaultAsync(t => t.Id == trackId);

            if (track == null)
                return ApiResponse<MetadataValidationResultDto>.ErrorResult("Track not found");

            var result = new MetadataValidationResultDto
            {
                TrackId = trackId,
                FilePath = track.FilePath,
                MissingFields = new List<string>(),
                InvalidFields = new List<string>(),
                Warnings = new List<string>()
            };

            // Check for missing required fields
            if (string.IsNullOrWhiteSpace(track.Title))
                result.MissingFields.Add("Title");

            if (track.Artist == null || string.IsNullOrWhiteSpace(track.Artist.Name))
                result.MissingFields.Add("Artist");

            if (track.Album == null || string.IsNullOrWhiteSpace(track.Album.Title))
                result.MissingFields.Add("Album");

            if (track.Genre == null || string.IsNullOrWhiteSpace(track.Genre.Name))
                result.MissingFields.Add("Genre");

            // Check for invalid fields
            if (track.Year.HasValue && (track.Year < 1900 || track.Year > DateTime.Now.Year + 1))
                result.InvalidFields.Add("Year");

            if (track.TrackNumber.HasValue && track.TrackNumber <= 0)
                result.InvalidFields.Add("TrackNumber");

            if (track.DiscNumber.HasValue && track.DiscNumber <= 0)
                result.InvalidFields.Add("DiscNumber");

            // Add warnings for potential issues
            if (!track.Year.HasValue)
                result.Warnings.Add("Year not specified");

            if (!track.TrackNumber.HasValue)
                result.Warnings.Add("Track number not specified");

            if (!File.Exists(track.FilePath))
                result.InvalidFields.Add("FilePath - File not found");

            return ApiResponse<MetadataValidationResultDto>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<MetadataValidationResultDto>.ErrorResult($"Error validating track metadata: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<MetadataValidationResultDto>>> ValidateMultipleTracksMetadataAsync(List<int> trackIds)
    {
        var results = new List<MetadataValidationResultDto>();

        try
        {
            foreach (var trackId in trackIds)
            {
                var validationResult = await ValidateTrackMetadataAsync(trackId);
                if (validationResult.Success && validationResult.Data != null)
                {
                    results.Add(validationResult.Data);
                }
            }

            return ApiResponse<List<MetadataValidationResultDto>>.SuccessResult(results);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<MetadataValidationResultDto>>.ErrorResult($"Error validating multiple tracks: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<MetadataValidationResultDto>>> GetTracksWithMetadataIssuesAsync()
    {
        try
        {
            var tracks = await _context.Tracks
                .Include(t => t.Artist)
                .Include(t => t.Album)
                .Include(t => t.Genre)
                .ToListAsync();

            var issueResults = new List<MetadataValidationResultDto>();

            foreach (var track in tracks)
            {
                var validationResult = await ValidateTrackMetadataAsync(track.Id);
                if (validationResult.Success && validationResult.Data != null)
                {
                    var validation = validationResult.Data;
                    if (validation.MissingFields.Any() || validation.InvalidFields.Any())
                    {
                        issueResults.Add(validation);
                    }
                }
            }

            return ApiResponse<List<MetadataValidationResultDto>>.SuccessResult(issueResults);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<MetadataValidationResultDto>>.ErrorResult($"Error getting tracks with metadata issues: {ex.Message}");
        }
    }

    public async Task<ApiResponse<MetadataEditResultDto>> RevertTrackMetadataAsync(int trackId)
    {
        try
        {
            var track = await _context.Tracks
                .Include(t => t.Metadata)
                .FirstOrDefaultAsync(t => t.Id == trackId);

            if (track == null)
                return ApiResponse<MetadataEditResultDto>.ErrorResult("Track not found");

            if (!File.Exists(track.FilePath))
                return ApiResponse<MetadataEditResultDto>.ErrorResult("Audio file not found");

            // Extract fresh metadata from the file
            var freshTrack = _metadataExtractionService.ExtractMetadata(track.FilePath);

            // Update database with fresh metadata
            track.Title = freshTrack.Title;
            track.TrackNumber = freshTrack.TrackNumber;
            track.DiscNumber = freshTrack.DiscNumber;
            track.Year = freshTrack.Year;
            track.Duration = freshTrack.Duration;
            track.BitRate = freshTrack.BitRate;
            track.SampleRate = freshTrack.SampleRate;

            // Update artist, album, genre
            if (freshTrack.Artist != null)
                track.Artist = await GetOrCreateArtistAsync(freshTrack.Artist.Name);

            if (freshTrack.Album != null)
                track.Album = await GetOrCreateAlbumAsync(freshTrack.Album.Title, freshTrack.Album.Year);

            if (freshTrack.Genre != null)
                track.Genre = await GetOrCreateGenreAsync(freshTrack.Genre.Name);

            // Clear and update metadata
            _context.RemoveRange(track.Metadata);
            track.Metadata = freshTrack.Metadata;

            track.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var result = new MetadataEditResultDto
            {
                TrackId = trackId,
                FilePath = track.FilePath,
                Success = true,
                UpdatedFields = new List<string> { "All metadata reverted from file" }
            };

            return ApiResponse<MetadataEditResultDto>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<MetadataEditResultDto>.ErrorResult($"Error reverting track metadata: {ex.Message}");
        }
    }

    private async Task<Artist> GetOrCreateArtistAsync(string name)
    {
        var artist = await _context.Artists.FirstOrDefaultAsync(a => a.Name == name);
        if (artist == null)
        {
            artist = new Artist { Name = name };
            _context.Artists.Add(artist);
            await _context.SaveChangesAsync();
        }
        return artist;
    }

    private async Task<Album> GetOrCreateAlbumAsync(string title, int? year = null)
    {
        var album = await _context.Albums.FirstOrDefaultAsync(a => a.Title == title);
        if (album == null)
        {
            album = new Album { Title = title, Year = year };
            _context.Albums.Add(album);
            await _context.SaveChangesAsync();
        }
        return album;
    }

    private async Task<Genre> GetOrCreateGenreAsync(string name)
    {
        var genre = await _context.Genres.FirstOrDefaultAsync(g => g.Name == name);
        if (genre == null)
        {
            genre = new Genre { Name = name };
            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();
        }
        return genre;
    }

    private async Task UpdateAdditionalMetadataAsync(Track track, string key, string value)
    {
        var metadata = track.Metadata.FirstOrDefault(m => m.Key == key);
        if (metadata == null)
        {
            metadata = new Metadata { Key = key, Value = value };
            track.Metadata.Add(metadata);
        }
        else
        {
            metadata.Value = value;
        }
    }

    private static List<string> ValidateMetadata(EditTrackMetadataDto metadata)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(metadata.Title))
            errors.Add("Title is required");

        if (metadata.Year.HasValue && (metadata.Year < 1900 || metadata.Year > DateTime.Now.Year + 1))
            errors.Add("Year must be between 1900 and " + (DateTime.Now.Year + 1));

        if (metadata.TrackNumber.HasValue && metadata.TrackNumber <= 0)
            errors.Add("Track number must be greater than 0");

        if (metadata.DiscNumber.HasValue && metadata.DiscNumber <= 0)
            errors.Add("Disc number must be greater than 0");

        return errors;
    }
}