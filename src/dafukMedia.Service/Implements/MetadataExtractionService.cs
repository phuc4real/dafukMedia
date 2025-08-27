using dafukMedia.Data.Models;
using dafukMedia.Service.Common;
using dafukMedia.Service.Interfaces;
using System.Security.Cryptography;

namespace dafukMedia.Service.Implements;

public class MetadataExtractionService : IMetadataExtractionService
{
    private readonly string[] _supportedFormats = {
        FileExtensionContants.MP3,
        FileExtensionContants.AAC,
        FileExtensionContants.Ogg,
        FileExtensionContants.FLAC,
        FileExtensionContants.WAV,
        FileExtensionContants.ALAC
    };

    public Track ExtractMetadata(string filePath)
    {
        if (!System.IO.File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        if (!IsSupportedFormat(filePath))
            throw new NotSupportedException($"Unsupported file format: {Path.GetExtension(filePath)}");

        var track = new Track
        {
            FilePath = filePath,
            FileFormat = Path.GetExtension(filePath).TrimStart('.').ToUpper(),
            FileSize = new FileInfo(filePath).Length,
            FileHash = CalculateFileHash(filePath)
        };

        try
        {
            using var file = TagLib.File.Create(filePath);

            // Basic metadata
            track.Title = CleanString(file.Tag.Title) ?? Path.GetFileNameWithoutExtension(filePath);
            track.TrackNumber = (int?)file.Tag.Track;
            track.DiscNumber = (int?)file.Tag.Disc;
            track.Year = (int?)file.Tag.Year;
            track.Duration = file.Properties.Duration;
            track.BitRate = file.Properties.AudioBitrate;
            track.SampleRate = file.Properties.AudioSampleRate;

            // Store additional metadata
            var metadataList = new List<Metadata>();

            if (!string.IsNullOrWhiteSpace(file.Tag.Comment))
            {
                metadataList.Add(new Metadata { Key = "COMMENT", Value = file.Tag.Comment });
            }

            if (!string.IsNullOrWhiteSpace(file.Tag.Lyrics))
            {
                metadataList.Add(new Metadata { Key = "LYRICS", Value = file.Tag.Lyrics });
            }

            if (file.Tag.Composers?.Length > 0)
            {
                metadataList.Add(new Metadata { Key = "COMPOSER", Value = string.Join(", ", file.Tag.Composers) });
            }

            if (file.Tag.Performers?.Length > 0)
            {
                metadataList.Add(new Metadata { Key = "PERFORMER", Value = string.Join(", ", file.Tag.Performers) });
            }

            track.Metadata = metadataList;

            // Extract artist, album, and genre info (we'll handle entity creation in the scanner service)
            if (file.Tag.FirstPerformer != null)
            {
                track.Artist = new Artist { Name = CleanString(file.Tag.FirstPerformer) };
            }

            if (file.Tag.Album != null)
            {
                track.Album = new Album
                {
                    Title = CleanString(file.Tag.Album),
                    Year = (int?)file.Tag.Year
                };
            }

            if (file.Tag.FirstGenre != null)
            {
                track.Genre = new Genre { Name = CleanString(file.Tag.FirstGenre) };
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't fail completely
            Console.WriteLine($"Error extracting metadata from {filePath}: {ex.Message}");

            // At least we have basic file info
            if (string.IsNullOrEmpty(track.Title))
                track.Title = Path.GetFileNameWithoutExtension(filePath);
        }

        return track;
    }

    public bool IsSupportedFormat(string filePath)
    {
        var extension = Path.GetExtension(filePath).TrimStart('.').ToLower();
        return _supportedFormats.Contains(extension);
    }

    public string CalculateFileHash(string filePath)
    {
        using var md5 = MD5.Create();
        using var stream = System.IO.File.OpenRead(filePath);
        var hash = md5.ComputeHash(stream);
        return Convert.ToHexString(hash);
    }

    public byte[]? ExtractAlbumArt(string filePath)
    {
        try
        {
            using var file = TagLib.File.Create(filePath);
            var pictures = file.Tag.Pictures;

            if (pictures?.Length > 0)
            {
                // Return the first picture (usually album art)
                return pictures[0].Data.Data;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting album art from {filePath}: {ex.Message}");
        }

        return null;
    }

    private static string? CleanString(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        return input.Trim();
    }
}