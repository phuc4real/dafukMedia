using Microsoft.AspNetCore.Mvc;
using dafukMedia.DTO;
using dafukMedia.Service.Interfaces;

namespace dafukMedia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScanController : ControllerBase
{
    private readonly IScannerService _scannerService;

    public ScanController(IScannerService scannerService)
    {
        _scannerService = scannerService;
    }

    /// <summary>
    /// Start a new scan job for the specified directory
    /// </summary>
    [HttpPost("start")]
    public async Task<ActionResult<ApiResponse<ScanJobDto>>> StartScan([FromBody] ScanJobCreateDto request)
    {
        if (string.IsNullOrWhiteSpace(request.DirectoryPath))
        {
            return BadRequest(ApiResponse<ScanJobDto>.ErrorResult("Directory path is required"));
        }

        var result = await _scannerService.StartScanAsync(request.DirectoryPath);
        
        if (result.Success)
            return Ok(result);
        
        return BadRequest(result);
    }

    /// <summary>
    /// Get all scan jobs
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ScanJobDto>>>> GetScanJobs()
    {
        var result = await _scannerService.GetScanJobsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get a specific scan job by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ScanJobDto>>> GetScanJob(int id)
    {
        var result = await _scannerService.GetScanJobAsync(id);
        
        if (result.Success)
            return Ok(result);
        
        return NotFound(result);
    }

    /// <summary>
    /// Get the progress of a running scan job
    /// </summary>
    [HttpGet("{id}/progress")]
    public async Task<ActionResult<ApiResponse<ScanProgressDto>>> GetScanProgress(int id)
    {
        var result = await _scannerService.GetScanProgressAsync(id);
        
        if (result.Success)
            return Ok(result);
        
        return NotFound(result);
    }

    /// <summary>
    /// Cancel a running scan job
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<ApiResponse<bool>>> CancelScan(int id)
    {
        var result = await _scannerService.CancelScanAsync(id);
        
        if (result.Success)
            return Ok(result);
        
        return BadRequest(result);
    }

    /// <summary>
    /// Delete a scan job
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteScanJob(int id)
    {
        var result = await _scannerService.DeleteScanJobAsync(id);
        
        if (result.Success)
            return Ok(result);
        
        return NotFound(result);
    }
}