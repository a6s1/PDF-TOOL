using PDFToolsPro.Models;

namespace PDFToolsPro.Services;

public interface IPdfService
{
    Task<int> GetPageCountAsync(string filePath);
}

public interface IPdfCompressorService : IPdfService
{
    Task<(bool Success, long OriginalSize, long NewSize, string? ErrorMessage)> CompressAsync(
        string inputPath, 
        string outputPath, 
        CompressionSettings settings,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);
}

public interface IPdfMergerService : IPdfService
{
    Task<(bool Success, string? ErrorMessage)> MergeAsync(
        IEnumerable<string> inputPaths, 
        string outputPath,
        bool compress = false,
        CompressionLevel compressionLevel = CompressionLevel.Medium,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);
}

public interface IPdfSplitterService : IPdfService
{
    Task<(bool Success, string? ErrorMessage)> SplitByRangeAsync(
        string inputPath, 
        string outputPath, 
        int startPage, 
        int endPage,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? ErrorMessage)> SplitEachPageAsync(
        string inputPath, 
        string outputDirectory,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? ErrorMessage)> ExtractPagesAsync(
        string inputPath, 
        string outputPath, 
        int[] pages,
        CancellationToken cancellationToken = default);
}

public interface IWatermarkService : IPdfService
{
    Task<(bool Success, string? ErrorMessage)> AddWatermarkAsync(
        string inputPath, 
        string outputPath, 
        WatermarkSettings settings,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);
}

public interface IEncryptionService : IPdfService
{
    Task<(bool Success, string? ErrorMessage)> EncryptAsync(
        string inputPath, 
        string outputPath, 
        ProtectionSettings settings,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? ErrorMessage)> DecryptAsync(
        string inputPath, 
        string outputPath, 
        string password,
        CancellationToken cancellationToken = default);
}



