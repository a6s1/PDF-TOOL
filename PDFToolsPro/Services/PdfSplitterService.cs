using System.IO;
using iText.Kernel.Pdf;

namespace PDFToolsPro.Services;

public class PdfSplitterService : IPdfSplitterService
{
    public async Task<int> GetPageCountAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            using var reader = new PdfReader(filePath);
            using var pdfDoc = new PdfDocument(reader);
            return pdfDoc.GetNumberOfPages();
        });
    }

    public async Task<(bool Success, string? ErrorMessage)> SplitByRangeAsync(
        string inputPath,
        string outputPath,
        int startPage,
        int endPage,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var reader = new PdfReader(inputPath);
                using var srcDoc = new PdfDocument(reader);

                var totalPages = srcDoc.GetNumberOfPages();
                
                if (startPage < 1) startPage = 1;
                if (endPage > totalPages) endPage = totalPages;
                if (startPage > endPage) 
                    throw new ArgumentException("Start page cannot be greater than end page");

                using var writer = new PdfWriter(outputPath);
                using var destDoc = new PdfDocument(writer);

                srcDoc.CopyPagesTo(startPage, endPage, destDoc);

            }, cancellationToken);

            return (true, null);
        }
        catch (OperationCanceledException)
        {
            CleanupFile(outputPath);
            return (false, "Operation was cancelled");
        }
        catch (Exception ex)
        {
            CleanupFile(outputPath);
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> SplitEachPageAsync(
        string inputPath,
        string outputDirectory,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            var fileName = Path.GetFileNameWithoutExtension(inputPath);

            await Task.Run(() =>
            {
                using var reader = new PdfReader(inputPath);
                using var srcDoc = new PdfDocument(reader);

                var totalPages = srcDoc.GetNumberOfPages();

                for (int i = 1; i <= totalPages; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var outputPath = Path.Combine(outputDirectory, $"{fileName}_page_{i}.pdf");
                    
                    using var writer = new PdfWriter(outputPath);
                    using var destDoc = new PdfDocument(writer);
                    srcDoc.CopyPagesTo(i, i, destDoc);

                    progress?.Report((int)((double)i / totalPages * 100));
                }

            }, cancellationToken);

            return (true, null);
        }
        catch (OperationCanceledException)
        {
            return (false, "Operation was cancelled");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> ExtractPagesAsync(
        string inputPath,
        string outputPath,
        int[] pages,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var reader = new PdfReader(inputPath);
                using var srcDoc = new PdfDocument(reader);

                var totalPages = srcDoc.GetNumberOfPages();
                var validPages = pages.Where(p => p >= 1 && p <= totalPages).Distinct().OrderBy(p => p).ToList();

                if (validPages.Count == 0)
                    throw new ArgumentException("No valid pages specified");

                using var writer = new PdfWriter(outputPath);
                using var destDoc = new PdfDocument(writer);

                foreach (var page in validPages)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    srcDoc.CopyPagesTo(page, page, destDoc);
                }

            }, cancellationToken);

            return (true, null);
        }
        catch (OperationCanceledException)
        {
            CleanupFile(outputPath);
            return (false, "Operation was cancelled");
        }
        catch (Exception ex)
        {
            CleanupFile(outputPath);
            return (false, ex.Message);
        }
    }

    private void CleanupFile(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch { }
    }
}

