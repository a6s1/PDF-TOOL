using System.IO;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Extgstate;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.IO.Image;
using PDFToolsPro.Models;

namespace PDFToolsPro.Services;

public class WatermarkService : IWatermarkService
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

    public async Task<(bool Success, string? ErrorMessage)> AddWatermarkAsync(
        string inputPath,
        string outputPath,
        WatermarkSettings settings,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Run(() =>
            {
                using var reader = new PdfReader(inputPath);
                using var writer = new PdfWriter(outputPath);
                using var pdfDoc = new PdfDocument(reader, writer);

                var numberOfPages = pdfDoc.GetNumberOfPages();

                for (int i = 1; i <= numberOfPages; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var page = pdfDoc.GetPage(i);
                    var pageSize = page.GetPageSize();
                    var canvas = new PdfCanvas(page);

                    // Create transparency
                    var gs = new PdfExtGState();
                    gs.SetFillOpacity(settings.Opacity);
                    canvas.SetExtGState(gs);

                    if (settings.Type == WatermarkType.Text)
                    {
                        AddTextWatermark(canvas, pageSize, settings);
                    }
                    else
                    {
                        AddImageWatermark(canvas, pageSize, settings);
                    }

                    progress?.Report((int)((double)i / numberOfPages * 100));
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

    private void AddTextWatermark(PdfCanvas canvas, Rectangle pageSize, WatermarkSettings settings)
    {
        var (x, y) = GetPosition(pageSize, settings.Position);

        canvas.SaveState();

        // Rotate around the position
        var radians = settings.Angle * Math.PI / 180;
        canvas.ConcatMatrix(
            (float)Math.Cos(radians), (float)Math.Sin(radians),
            -(float)Math.Sin(radians), (float)Math.Cos(radians),
            x, y);

        var font = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
        canvas.BeginText()
            .SetFontAndSize(font, settings.FontSize)
            .SetTextMatrix(0, 0)
            .ShowText(settings.Text)
            .EndText();

        canvas.RestoreState();
    }

    private void AddImageWatermark(PdfCanvas canvas, Rectangle pageSize, WatermarkSettings settings)
    {
        if (!File.Exists(settings.ImagePath))
            return;

        var (x, y) = GetPosition(pageSize, settings.Position);

        canvas.SaveState();

        var imageData = ImageDataFactory.Create(settings.ImagePath);
        var image = new iText.Layout.Element.Image(imageData);

        // Scale image to reasonable size (max 200px)
        var scaleFactor = Math.Min(200f / imageData.GetWidth(), 200f / imageData.GetHeight());
        var width = imageData.GetWidth() * scaleFactor;
        var height = imageData.GetHeight() * scaleFactor;

        // Adjust position based on image size
        x -= width / 2;
        y -= height / 2;

        // Apply rotation
        var radians = settings.Angle * Math.PI / 180;
        canvas.ConcatMatrix(
            (float)Math.Cos(radians), (float)Math.Sin(radians),
            -(float)Math.Sin(radians), (float)Math.Cos(radians),
            x + width / 2, y + height / 2);

        canvas.AddImageAt(imageData, -width / 2, -height / 2, false);

        canvas.RestoreState();
    }

    private (float x, float y) GetPosition(Rectangle pageSize, WatermarkPosition position)
    {
        var margin = 50f;
        
        return position switch
        {
            WatermarkPosition.Center => (pageSize.GetWidth() / 2, pageSize.GetHeight() / 2),
            WatermarkPosition.TopLeft => (margin, pageSize.GetHeight() - margin),
            WatermarkPosition.TopRight => (pageSize.GetWidth() - margin, pageSize.GetHeight() - margin),
            WatermarkPosition.BottomLeft => (margin, margin),
            WatermarkPosition.BottomRight => (pageSize.GetWidth() - margin, margin),
            _ => (pageSize.GetWidth() / 2, pageSize.GetHeight() / 2)
        };
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

