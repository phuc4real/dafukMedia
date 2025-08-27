using dafukMedia.DTO;

namespace dafukMedia.Service.Interfaces
{
    public interface IScannerService
    {
        Task<ApiResponse<ScanJobDto>> StartScanAsync(string directoryPath);
        Task<ApiResponse<ScanJobDto>> GetScanJobAsync(int jobId);
        Task<ApiResponse<List<ScanJobDto>>> GetScanJobsAsync();
        Task<ApiResponse<ScanProgressDto>> GetScanProgressAsync(int jobId);
        Task<ApiResponse<bool>> CancelScanAsync(int jobId);
        Task<ApiResponse<bool>> DeleteScanJobAsync(int jobId);
    }
}
