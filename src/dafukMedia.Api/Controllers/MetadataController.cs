using Microsoft.AspNetCore.Mvc;
using dafukMedia.DTO;
using dafukMedia.Service.Interfaces;

namespace dafukMedia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetadataController : ControllerBase
{
    private readonly IMetadataEditService _metadataEditService;

    public MetadataController(IMetadataEditService metadataEditService)
    {
        _metadataEditService = metadataEditService;
    }

    /// <summary>
    /// Get detailed metadata for a specific track
    /// </summary>
    [HttpGet("track/{id}")]
    public async Task<ActionResult<ApiResponse<TrackMetadataDto>>> GetTrackMetadata(int id)
    {
        var result = await _metadataEditService.GetTrackMetadataAsync(id);
        
        if (result.Success)
            return Ok(result);
        
        return NotFound(result);
    }

    /// <summary>
    /// Edit metadata for a single track
    /// </summary>
    [HttpPut("track/{id}")]
    public async Task<ActionResult<ApiResponse<MetadataEditResultDto>>> EditTrackMetadata(int id, [FromBody] EditTrackMetadataDto metadata)
    {
        if (string.IsNullOrWhiteSpace(metadata.Title))
        {
            return BadRequest(ApiResponse<MetadataEditResultDto>.ErrorResult("Title is required"));
        }

        var result = await _metadataEditService.EditTrackMetadataAsync(id, metadata);
        
        if (result.Success)
            return Ok(result);
        
        return BadRequest(result);
    }

    /// <summary>
    /// Edit metadata for multiple tracks (batch operation)
    /// </summary>
    [HttpPut("batch")]
    public async Task<ActionResult<ApiResponse<List<MetadataEditResultDto>>>> BatchEditMetadata([FromBody] BatchEditMetadataDto batchRequest)
    {
        if (batchRequest.TrackIds == null || !batchRequest.TrackIds.Any())
        {
            return BadRequest(ApiResponse<List<MetadataEditResultDto>>.ErrorResult("At least one track ID is required"));
        }

        if (string.IsNullOrWhiteSpace(batchRequest.Metadata.Title) && 
            string.IsNullOrWhiteSpace(batchRequest.Metadata.Artist) &&
            string.IsNullOrWhiteSpace(batchRequest.Metadata.Album) &&
            string.IsNullOrWhiteSpace(batchRequest.Metadata.Genre) &&
            !batchRequest.Metadata.Year.HasValue &&
            !batchRequest.Metadata.TrackNumber.HasValue &&
            !batchRequest.Metadata.DiscNumber.HasValue &&
            string.IsNullOrWhiteSpace(batchRequest.Metadata.Comment) &&
            string.IsNullOrWhiteSpace(batchRequest.Metadata.Composer) &&
            string.IsNullOrWhiteSpace(batchRequest.Metadata.AlbumArtist) &&
            !batchRequest.Metadata.AdditionalMetadata.Any())
        {
            return BadRequest(ApiResponse<List<MetadataEditResultDto>>.ErrorResult("At least one metadata field must be provided"));
        }

        var result = await _metadataEditService.BatchEditMetadataAsync(batchRequest);
        return Ok(result);
    }

    /// <summary>
    /// Validate metadata for a specific track and highlight missing/invalid fields
    /// </summary>
    [HttpGet("track/{id}/validate")]
    public async Task<ActionResult<ApiResponse<MetadataValidationResultDto>>> ValidateTrackMetadata(int id)
    {
        var result = await _metadataEditService.ValidateTrackMetadataAsync(id);
        
        if (result.Success)
            return Ok(result);
        
        return NotFound(result);
    }

    /// <summary>
    /// Validate metadata for multiple tracks
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult<ApiResponse<List<MetadataValidationResultDto>>>> ValidateMultipleTracksMetadata([FromBody] List<int> trackIds)
    {
        if (trackIds == null || !trackIds.Any())
        {
            return BadRequest(ApiResponse<List<MetadataValidationResultDto>>.ErrorResult("At least one track ID is required"));
        }

        var result = await _metadataEditService.ValidateMultipleTracksMetadataAsync(trackIds);
        return Ok(result);
    }

    /// <summary>
    /// Get all tracks with metadata issues (missing or invalid data)
    /// </summary>
    [HttpGet("issues")]
    public async Task<ActionResult<ApiResponse<List<MetadataValidationResultDto>>>> GetTracksWithMetadataIssues()
    {
        var result = await _metadataEditService.GetTracksWithMetadataIssuesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Revert metadata changes for a track (restore from file)
    /// </summary>
    [HttpPost("track/{id}/revert")]
    public async Task<ActionResult<ApiResponse<MetadataEditResultDto>>> RevertTrackMetadata(int id)
    {
        var result = await _metadataEditService.RevertTrackMetadataAsync(id);
        
        if (result.Success)
            return Ok(result);
        
        return BadRequest(result);
    }
}