using System.IO;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Xobject;
using PDFToolsPro.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace PDFToolsPro.Services;

public class PdfCompressorService : IPdfCompressorService
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

    public async Task<(bool Success, long OriginalSize, long NewSize, string? ErrorMessage)> CompressAsync(
        string inputPath,
        string outputPath,
        CompressionSettings settings,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate input file exists
            if (!File.Exists(inputPath))
            {
                return (false, 0, 0, "Input file not found");
            }

            var originalSize = new FileInfo(inputPath).Length;

            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            await Task.Run(() =>
            {
                using var reader = new PdfReader(inputPath);
                var writerProperties = new WriterProperties();
                
                // Set compression level
                writerProperties.SetCompressionLevel(CompressionConstants.BEST_COMPRESSION);
                writerProperties.SetFullCompressionMode(true);

                using var writer = new PdfWriter(outputPath, writerProperties);
                using var pdfDoc = new PdfDocument(reader, writer);

                var numberOfPages = pdfDoc.GetNumberOfPages();

                for (int i = 1; i <= numberOfPages; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var page = pdfDoc.GetPage(i);
                        var resources = page.GetResources();
                        var xObjects = resources?.GetResource(PdfName.XObject);

                        if (xObjects != null)
                        {
                            CompressImages(xObjects, settings);
                        }

                        // Remove metadata if specified
                        if (settings.RemoveMetadata)
                        {
                            page.GetPdfObject().Remove(PdfName.Metadata);
                        }
                    }
                    catch
                    {
                        // Skip problematic pages
                    }

                    progress?.Report((int)((double)i / numberOfPages * 100));
                }

                // Remove document-level metadata
                if (settings.RemoveMetadata)
                {
                    try
                    {
                        pdfDoc.GetCatalog().GetPdfObject().Remove(PdfName.Metadata);
                        var info = pdfDoc.GetDocumentInfo();
                        info.SetAuthor("");
                        info.SetCreator("");
                        info.SetKeywords("");
                        info.SetSubject("");
                        info.SetTitle("");
                    }
                    catch
                    {
                        // Ignore metadata removal errors
                    }
                }

            }, cancellationToken);

            var newSize = new FileInfo(outputPath).Length;
            return (true, originalSize, newSize, null);
        }
        catch (OperationCanceledException)
        {
            TryDeleteFile(outputPath);
            return (false, 0, 0, "Operation was cancelled");
        }
        catch (Exception ex)
        {
            TryDeleteFile(outputPath);
            return (false, 0, 0, ex.Message);
        }
    }

    private void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // Ignore deletion errors
        }
    }

    private void CompressImages(PdfObject xObjects, CompressionSettings settings)
    {
        if (xObjects is not PdfDictionary xObjectDict)
            return;

        foreach (var name in xObjectDict.KeySet().ToList())
        {
            var xObject = xObjectDict.GetAsStream(name);
            if (xObject == null) continue;

            var subtype = xObject.GetAsName(PdfName.Subtype);
            if (PdfName.Image.Equals(subtype))
            {
                try
                {
                    var imageXObject = new PdfImageXObject(xObject);
                    var imageBytes = imageXObject.GetImageBytes();

                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        using var image = Image.Load(imageBytes);
                        
                        // Resize if needed
                        if (settings.ScaleFactor < 1.0f)
                        {
                            var newWidth = (int)(image.Width * settings.ScaleFactor);
                            var newHeight = (int)(image.Height * settings.ScaleFactor);
                            image.Mutate(x => x.Resize(newWidth, newHeight));
                        }

                        using var ms = new MemoryStream();
                        var encoder = new JpegEncoder { Quality = settings.ImageQuality };
                        image.SaveAsJpeg(ms, encoder);

                        // Only replace if the new size is smaller
                        if (ms.Length < imageBytes.Length)
                        {
                            var newImageBytes = ms.ToArray();
                            xObject.SetData(newImageBytes);
                            xObject.Put(PdfName.Filter, PdfName.DCTDecode);
                        }
                    }
                }
                catch
                {
                    // Skip images that can't be processed
                }
            }
        }
    }
}

