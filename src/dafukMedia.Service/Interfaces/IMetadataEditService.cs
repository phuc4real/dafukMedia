using dafukMedia.DTO;

namespace dafukMedia.Service.Interfaces;

public interface IMetadataEditService
{
    /// <summary>
    /// Get detailed metadata for a specific track
    /// </summary>
    Task<ApiResponse<TrackMetadataDto>> GetTrackMetadataAsync(int trackId);

    /// <summary>
    /// Edit metadata for a single track
    /// </summary>
    Task<ApiResponse<MetadataEditResultDto>> EditTrackMetadataAsync(int trackId, EditTrackMetadataDto metadata);

    /// <summary>
    /// Edit metadata for multiple tracks (batch operation)
    /// </summary>
    Task<ApiResponse<List<MetadataEditResultDto>>> BatchEditMetadataAsync(BatchEditMetadataDto batchRequest);

    /// <summary>
    /// Validate metadata for a track and highlight missing/invalid fields
    /// </summary>
    Task<ApiResponse<MetadataValidationResultDto>> ValidateTrackMetadataAsync(int trackId);

    /// <summary>
    /// Validate metadata for multiple tracks
    /// </summary>
    Task<ApiResponse<List<MetadataValidationResultDto>>> ValidateMultipleTracksMetadataAsync(List<int> trackIds);

    /// <summary>
    /// Get all tracks with metadata issues (missing or invalid data)
    /// </summary>
    Task<ApiResponse<List<MetadataValidationResultDto>>> GetTracksWithMetadataIssuesAsync();

    /// <summary>
    /// Revert metadata changes for a track (restore from file)
    /// </summary>
    Task<ApiResponse<MetadataEditResultDto>> RevertTrackMetadataAsync(int trackId);
}