using System.IO;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using PDFToolsPro.Models;

namespace PDFToolsPro.Services;

public class PdfMergerService : IPdfMergerService
{
    private readonly IPdfCompressorService _compressor;
    private const int BATCH_SIZE = 20; // Merge 20 files at a time to prevent memory issues

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
            return (false, "لا توجد ملفات للدمج");

        System.Diagnostics.Debug.WriteLine($"MergeAsync: {totalFiles} files to {outputPath}");

        try
        {
            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            // Simple direct merge
            await Task.Run(() =>
            {
                using var writer = new PdfWriter(outputPath);
                using var pdfDoc = new PdfDocument(writer);
                var merger = new PdfMerger(pdfDoc);

                for (int i = 0; i < totalFiles; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    var currentFile = paths[i];
                    System.Diagnostics.Debug.WriteLine($"  Merging: {currentFile}");
                    
                    if (!File.Exists(currentFile))
                    {
                        System.Diagnostics.Debug.WriteLine($"  File not found: {currentFile}");
                        continue;
                    }

                    using var reader = new PdfReader(currentFile);
                    using var srcDoc = new PdfDocument(reader);
                    merger.Merge(srcDoc, 1, srcDoc.GetNumberOfPages());

                    progress?.Report((int)((i + 1) * 100.0 / totalFiles));
                }
                
                // Close explicitly
                pdfDoc.Close();
                System.Diagnostics.Debug.WriteLine($"  PDF closed");
                
            }, cancellationToken);

            // Verify output file
            if (!File.Exists(outputPath))
            {
                return (false, $"الملف لم يُنشأ: {outputPath}");
            }

            var info = new FileInfo(outputPath);
            System.Diagnostics.Debug.WriteLine($"  Output file size: {info.Length} bytes");
            
            if (info.Length == 0)
            {
                return (false, "الملف المدمج فارغ");
            }

            // Compress if requested
            if (compress)
            {
                var tempPath = outputPath + ".temp";
                File.Move(outputPath, tempPath);
                
                var compressionProgress = new Progress<int>(p => progress?.Report(50 + p / 2));
                var compressResult = await _compressor.CompressAsync(
                    tempPath,
                    outputPath,
                    new CompressionSettings { Level = compressionLevel },
                    compressionProgress,
                    cancellationToken);

                CleanupFiles(tempPath);

                if (!compressResult.Success)
                    return (false, compressResult.ErrorMessage);
            }

            progress?.Report(100);
            return (true, null);
        }
        catch (OperationCanceledException)
        {
            CleanupFiles(outputPath);
            return (false, "تم إلغاء العملية");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"  Exception: {ex}");
            CleanupFiles(outputPath);
            return (false, ex.Message);
        }
    }

    private async Task<string> MergeInBatchesAsync(
        List<string> paths,
        bool willCompress,
        IProgress<int>? progress,
        CancellationToken cancellationToken)
    {
        var totalFiles = paths.Count;
        var batches = new List<List<string>>();
        var tempBatchFiles = new List<string>();

        // Split into batches
        for (int i = 0; i < totalFiles; i += BATCH_SIZE)
        {
            batches.Add(paths.Skip(i).Take(BATCH_SIZE).ToList());
        }

        var totalBatches = batches.Count;
        var progressPerBatch = (willCompress ? 50 : 100) / (totalBatches + 1); // +1 for final merge

        try
        {
            // Merge each batch
            for (int batchIndex = 0; batchIndex < totalBatches; batchIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var batchTempPath = Path.GetTempFileName() + $"_batch{batchIndex}.pdf";
                tempBatchFiles.Add(batchTempPath);

                var batchStartProgress = batchIndex * progressPerBatch;
                var batchProgress = new Progress<int>(p =>
                    progress?.Report(batchStartProgress + (p * progressPerBatch / 100)));

                await MergeSingleBatchAsync(batches[batchIndex], batchTempPath, batchProgress, cancellationToken, 0, 100);

                // Force garbage collection after each batch to free memory
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            // Final merge of all batch results
            var finalTempPath = Path.GetTempFileName() + "_final.pdf";
            var finalProgress = new Progress<int>(p =>
                progress?.Report((totalBatches * progressPerBatch) + (p * progressPerBatch / 100)));

            await MergeSingleBatchAsync(tempBatchFiles, finalTempPath, finalProgress, cancellationToken, 0, 100);

            // Cleanup batch temp files
            CleanupFiles(tempBatchFiles.ToArray());

            return finalTempPath;
        }
        catch
        {
            CleanupFiles(tempBatchFiles.ToArray());
            throw;
        }
    }

    private async Task MergeSingleBatchAsync(
        List<string> paths,
        string outputPath,
        IProgress<int>? progress,
        CancellationToken cancellationToken,
        int progressStart,
        int progressEnd)
    {
        var totalFiles = paths.Count;
        
        if (totalFiles == 0)
            throw new Exception("لا توجد ملفات للدمج");

        await Task.Run(() =>
        {
            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var writerProperties = new WriterProperties();
            writerProperties.SetCompressionLevel(CompressionConstants.BEST_COMPRESSION);

            // Use explicit using blocks to ensure proper disposal
            PdfWriter? writer = null;
            PdfDocument? pdfDoc = null;
            
            try
            {
                writer = new PdfWriter(outputPath, writerProperties);
                pdfDoc = new PdfDocument(writer);
                var merger = new PdfMerger(pdfDoc);

                int mergedCount = 0;
                for (int i = 0; i < totalFiles; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var currentFile = paths[i];
                    
                    // Skip if file doesn't exist
                    if (!File.Exists(currentFile))
                    {
                        continue;
                    }

                    try
                    {
                        using var reader = new PdfReader(currentFile);
                        using var srcDoc = new PdfDocument(reader);
                        var pageCount = srcDoc.GetNumberOfPages();
                        if (pageCount > 0)
                        {
                            merger.Merge(srcDoc, 1, pageCount);
                            mergedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"خطأ في الملف: {Path.GetFileName(currentFile)} - {ex.Message}");
                    }

                    var currentProgress = progressStart + (int)((double)(i + 1) / totalFiles * (progressEnd - progressStart));
                    progress?.Report(currentProgress);
                }

                if (mergedCount == 0)
                    throw new Exception("لم يتم دمج أي ملف");
                    
                // Explicitly close the document to flush all content
                pdfDoc.Close();
                pdfDoc = null;
                writer = null;
            }
            finally
            {
                // Ensure disposal even if Close() was called
                pdfDoc?.Close();
            }
            
            // Verify file was created
            if (!File.Exists(outputPath))
                throw new Exception("فشل في إنشاء الملف المدمج");
                
            var fileInfo = new FileInfo(outputPath);
            if (fileInfo.Length == 0)
                throw new Exception("الملف المدمج فارغ");

        }, cancellationToken);
    }

    private void CleanupFiles(params string[] paths)
    {
        foreach (var path in paths)
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    File.Delete(path);
            }
            catch { }
        }
    }
}

