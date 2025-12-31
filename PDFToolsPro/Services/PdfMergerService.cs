using System.IO;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using PDFToolsPro.Models;

namespace PDFToolsPro.Services;

public class PdfMergerService : IPdfMergerService
{
    private readonly IPdfCompressorService _compressor;

    public PdfMergerService()
    {
        _compressor = new PdfCompressorService();
    }

    public async Task<int> GetPageCountAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            using var reader = new PdfReader(filePath);
            using var pdfDoc = new PdfDocument(reader);
            return pdfDoc.GetNumberOfPages();
        });
    }

    public async Task<(bool Success, string? ErrorMessage)> MergeAsync(
        IEnumerable<string> inputPaths,
        string outputPath,
        bool compress = false,
        CompressionLevel compressionLevel = CompressionLevel.Medium,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var paths = inputPaths.ToList();
        var totalFiles = paths.Count;

        if (totalFiles == 0)
            return (false, "لا توجد ملفات للدمج / No files to merge");

        if (totalFiles < 2)
            return (false, "يجب اختيار ملفين على الأقل / Select at least 2 files");

        try
        {
            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            // Delete existing file if exists
            if (File.Exists(outputPath))
                File.Delete(outputPath);

            await Task.Run(() =>
            {
                // Create writer with compression
                var writerProperties = new WriterProperties();
                writerProperties.SetCompressionLevel(CompressionConstants.BEST_COMPRESSION);
                
                // Create output document
                using (var writer = new PdfWriter(outputPath, writerProperties))
                using (var pdfDoc = new PdfDocument(writer))
                {
                    var merger = new PdfMerger(pdfDoc);

                    for (int i = 0; i < totalFiles; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        
                        var currentFile = paths[i];
                        
                        if (!File.Exists(currentFile))
                            continue;

                        // Open source document and merge
                        using (var reader = new PdfReader(currentFile))
                        using (var srcDoc = new PdfDocument(reader))
                        {
                            int pageCount = srcDoc.GetNumberOfPages();
                            if (pageCount > 0)
                            {
                                merger.Merge(srcDoc, 1, pageCount);
                            }
                        }

                        // Report progress
                        int progressValue = (int)((i + 1) * 100.0 / totalFiles);
                        progress?.Report(compress ? progressValue / 2 : progressValue);
                    }
                    
                    // Document is automatically closed and saved when disposed
                }

            }, cancellationToken);

            // Verify output file
            if (!File.Exists(outputPath))
            {
                return (false, "فشل في إنشاء الملف / Failed to create file");
            }

            var fileInfo = new FileInfo(outputPath);
            if (fileInfo.Length == 0)
            {
                File.Delete(outputPath);
                return (false, "الملف المدمج فارغ / Merged file is empty");
            }

            // Compress if requested
            if (compress)
            {
                var tempPath = Path.Combine(Path.GetDirectoryName(outputPath)!, $"temp_{Guid.NewGuid()}.pdf");
                
                try
                {
                    // Move original to temp
                    File.Move(outputPath, tempPath);
                    
                    var compressionProgress = new Progress<int>(p => progress?.Report(50 + p / 2));
                    var compressResult = await _compressor.CompressAsync(
                        tempPath,
                        outputPath,
                        new CompressionSettings { Level = compressionLevel },
                        compressionProgress,
                        cancellationToken);

                    // Cleanup temp file
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);

                    if (!compressResult.Success)
                    {
                        // Restore original if compression failed
                        if (File.Exists(tempPath) && !File.Exists(outputPath))
                            File.Move(tempPath, outputPath);
                        return (false, compressResult.ErrorMessage);
                    }
                }
                catch
                {
                    // Restore original if something went wrong
                    if (File.Exists(tempPath) && !File.Exists(outputPath))
                        File.Move(tempPath, outputPath);
                    throw;
                }
            }

            progress?.Report(100);
            return (true, null);
        }
        catch (OperationCanceledException)
        {
            CleanupFile(outputPath);
            return (false, "تم إلغاء العملية / Operation cancelled");
        }
        catch (Exception ex)
        {
            CleanupFile(outputPath);
            return (false, $"{ex.Message}");
        }
    }

    private void CleanupFile(string path)
    {
        try
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
                File.Delete(path);
        }
        catch { }
    }
}
