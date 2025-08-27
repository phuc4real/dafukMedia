using dafukMedia.Data.DBContext;
using dafukMedia.Data.Models;
using dafukMedia.DTO;
using dafukMedia.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace dafukMedia.Service.Implements;

public class ScannerService : IScannerService
{
    private readonly dafukMediaContext _context;
    private readonly IMetadataExtractionService _metadataService;
    private readonly ConcurrentDictionary<int, CancellationTokenSource> _activeScanJobs = new();
    private readonly ConcurrentDictionary<int, ScanProgressDto> _scanProgress = new();

    public ScannerService(dafukMediaContext context, IMetadataExtractionService metadataService)
    {
        _context = context;
        _metadataService = metadataService;
    }

    public async Task<ApiResponse<ScanJobDto>> StartScanAsync(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return ApiResponse<ScanJobDto>.ErrorResult("Directory does not exist");
        }

        var scanJob = new ScanJob
        {
            DirectoryPath = directoryPath,
            Status = "Running",
            StartTime = DateTime.UtcNow
        };

        _context.ScanJobs.Add(scanJob);
        await _context.SaveChangesAsync();

        var cancellationTokenSource = new CancellationTokenSource();
        _activeScanJobs[scanJob.Id] = cancellationTokenSource;

        // Initialize progress tracking
        _scanProgress[scanJob.Id] = new ScanProgressDto
        {
            JobId = scanJob.Id,
            Status = "Running",
            ProgressPercentage = 0
        };

        // Start scanning in background
        _ = Task.Run(async () => await PerformScanAsync(scanJob, cancellationTokenSource.Token));

        var dto = MapToDto(scanJob);
        return ApiResponse<ScanJobDto>.SuccessResult(dto, "Scan job started successfully");
    }

    public async Task<ApiResponse<ScanJobDto>> GetScanJobAsync(int jobId)
    {
        var scanJob = await _context.ScanJobs.FindAsync(jobId);
        if (scanJob == null)
        {
            return ApiResponse<ScanJobDto>.ErrorResult("Scan job not found");
        }

        var dto = MapToDto(scanJob);
        return ApiResponse<ScanJobDto>.SuccessResult(dto);
    }

    public async Task<ApiResponse<List<ScanJobDto>>> GetScanJobsAsync()
    {
        var scanJobs = await _context.ScanJobs
            .OrderByDescending(sj => sj.CreatedAt)
            .ToListAsync();

        var dtos = scanJobs.Select(MapToDto).ToList();
        return ApiResponse<List<ScanJobDto>>.SuccessResult(dtos);
    }

    public async Task<ApiResponse<ScanProgressDto>> GetScanProgressAsync(int jobId)
    {
        if (_scanProgress.TryGetValue(jobId, out var progress))
        {
            return ApiResponse<ScanProgressDto>.SuccessResult(progress);
        }

        var scanJob = await _context.ScanJobs.FindAsync(jobId);
        if (scanJob == null)
        {
            return ApiResponse<ScanProgressDto>.ErrorResult("Scan job not found");
        }

        // If job is not in active progress, create a progress object from the completed job
        var staticProgress = new ScanProgressDto
        {
            JobId = jobId,
            Status = scanJob.Status,
            FilesScanned = scanJob.FilesScanned,
            FilesAdded = scanJob.FilesAdded,
            DuplicatesFound = scanJob.DuplicatesFound,
            ErrorsCount = scanJob.ErrorsCount,
            ProgressPercentage = scanJob.Status == "Completed" ? 100 : 0
        };

        return ApiResponse<ScanProgressDto>.SuccessResult(staticProgress);
    }

    public async Task<ApiResponse<bool>> CancelScanAsync(int jobId)
    {
        if (_activeScanJobs.TryGetValue(jobId, out var cancellationTokenSource))
        {
            cancellationTokenSource.Cancel();
            _activeScanJobs.TryRemove(jobId, out _);
            _scanProgress.TryRemove(jobId, out _);

            var scanJob = await _context.ScanJobs.FindAsync(jobId);
            if (scanJob != null)
            {
                scanJob.Status = "Cancelled";
                scanJob.EndTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return ApiResponse<bool>.SuccessResult(true, "Scan job cancelled successfully");
        }

        return ApiResponse<bool>.ErrorResult("Scan job not found or not running");
    }

    public async Task<ApiResponse<bool>> DeleteScanJobAsync(int jobId)
    {
        var scanJob = await _context.ScanJobs.FindAsync(jobId);
        if (scanJob == null)
        {
            return ApiResponse<bool>.ErrorResult("Scan job not found");
        }

        // Cancel if it's running
        if (_activeScanJobs.ContainsKey(jobId))
        {
            await CancelScanAsync(jobId);
        }

        _context.ScanJobs.Remove(scanJob);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResult(true, "Scan job deleted successfully");
    }

    private async Task PerformScanAsync(ScanJob scanJob, CancellationToken cancellationToken)
    {
        try
        {
            var files = GetAudioFiles(scanJob.DirectoryPath);
            var totalFiles = files.Count;

            _scanProgress[scanJob.Id] = new ScanProgressDto
            {
                JobId = scanJob.Id,
                Status = "Running",
                ProgressPercentage = 0
            };

            for (int i = 0; i < files.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var file = files[i];
                
                // Update progress
                var progress = _scanProgress[scanJob.Id];
                progress.CurrentFile = Path.GetFileName(file);
                progress.ProgressPercentage = (double)(i + 1) / totalFiles * 100;
                progress.FilesScanned = i + 1;

                try
                {
                    var existingTrack = await _context.Tracks
                        .FirstOrDefaultAsync(t => t.FilePath == file, cancellationToken);

                    if (existingTrack != null)
                    {
                        progress.DuplicatesFound++;
                        continue;
                    }

                    var track = _metadataService.ExtractMetadata(file);
                    
                    // Handle entities (Artist, Album, Genre)
                    await ProcessEntitiesAsync(track);

                    _context.Tracks.Add(track);
                    await _context.SaveChangesAsync(cancellationToken);

                    progress.FilesAdded++;
                }
                catch (Exception ex)
                {
                    progress.ErrorsCount++;
                    Console.WriteLine($"Error processing file {file}: {ex.Message}");
                }

                // Update database periodically
                if (i % 10 == 0)
                {
                    scanJob.FilesScanned = progress.FilesScanned;
                    scanJob.FilesAdded = progress.FilesAdded;
                    scanJob.DuplicatesFound = progress.DuplicatesFound;
                    scanJob.ErrorsCount = progress.ErrorsCount;
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }

            // Final update
            scanJob.Status = cancellationToken.IsCancellationRequested ? "Cancelled" : "Completed";
            scanJob.EndTime = DateTime.UtcNow;
            scanJob.FilesScanned = _scanProgress[scanJob.Id].FilesScanned;
            scanJob.FilesAdded = _scanProgress[scanJob.Id].FilesAdded;
            scanJob.DuplicatesFound = _scanProgress[scanJob.Id].DuplicatesFound;
            scanJob.ErrorsCount = _scanProgress[scanJob.Id].ErrorsCount;

            if (_scanProgress.TryGetValue(scanJob.Id, out var finalProgress))
            {
                finalProgress.Status = scanJob.Status;
                finalProgress.ProgressPercentage = 100;
            }
        }
        catch (Exception ex)
        {
            scanJob.Status = "Failed";
            scanJob.ErrorMessage = ex.Message;
            scanJob.EndTime = DateTime.UtcNow;
            
            if (_scanProgress.TryGetValue(scanJob.Id, out var errorProgress))
            {
                errorProgress.Status = "Failed";
            }
        }
        finally
        {
            await _context.SaveChangesAsync();
            _activeScanJobs.TryRemove(scanJob.Id, out _);

            // Keep progress for a while for final status checks
            await Task.Delay(TimeSpan.FromMinutes(5)).ContinueWith(x => 
            _scanProgress.TryRemove(scanJob.Id, out _));
        }
    }

    private async Task ProcessEntitiesAsync(Track track)
    {
        // Process Artist
        if (track.Artist != null && !string.IsNullOrEmpty(track.Artist.Name))
        {
            var existingArtist = await _context.Artists
                .FirstOrDefaultAsync(a => a.Name == track.Artist.Name);

            if (existingArtist != null)
            {
                track.ArtistId = existingArtist.Id;
                track.Artist = null; // Don't create new entity
            }
            else
            {
                // Will be created as new entity
            }
        }

        // Process Album
        if (track.Album != null && !string.IsNullOrEmpty(track.Album.Title))
        {
            var existingAlbum = await _context.Albums
                .FirstOrDefaultAsync(a => a.Title == track.Album.Title && 
                                         (track.ArtistId == null || a.ArtistId == track.ArtistId));

            if (existingAlbum != null)
            {
                track.AlbumId = existingAlbum.Id;
                track.Album = null; // Don't create new entity
            }
            else
            {
                // Set album artist if track has artist
                if (track.ArtistId.HasValue)
                {
                    track.Album.ArtistId = track.ArtistId;
                }
            }
        }

        // Process Genre
        if (track.Genre != null && !string.IsNullOrEmpty(track.Genre.Name))
        {
            var existingGenre = await _context.Genres
                .FirstOrDefaultAsync(g => g.Name == track.Genre.Name);

            if (existingGenre != null)
            {
                track.GenreId = existingGenre.Id;
                track.Genre = null; // Don't create new entity
            }
            else
            {
                // Will be created as new entity
            }
        }
    }

    private List<string> GetAudioFiles(string directoryPath)
    {
        var supportedExtensions = new[] { "*.mp3", "*.flac", "*.wav", "*.aac", "*.ogg", "*.alac" };
        var files = new List<string>();

        foreach (var extension in supportedExtensions)
        {
            files.AddRange(Directory.GetFiles(directoryPath, extension, SearchOption.AllDirectories));
        }

        return files;
    }

    private static ScanJobDto MapToDto(ScanJob scanJob)
    {
        return new ScanJobDto
        {
            Id = scanJob.Id,
            DirectoryPath = scanJob.DirectoryPath,
            Status = scanJob.Status,
            StartTime = scanJob.StartTime,
            EndTime = scanJob.EndTime,
            FilesScanned = scanJob.FilesScanned,
            FilesAdded = scanJob.FilesAdded,
            DuplicatesFound = scanJob.DuplicatesFound,
            ErrorsCount = scanJob.ErrorsCount,
            ErrorMessage = scanJob.ErrorMessage
        };
    }
}