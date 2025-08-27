using dafukMedia.Data.Models;

namespace dafukMedia.Service.Interfaces;

public interface IMetadataExtractionService
{
    Track ExtractMetadata(string filePath);
    bool IsSupportedFormat(string filePath);
    string CalculateFileHash(string filePath);
    byte[]? ExtractAlbumArt(string filePath);
}