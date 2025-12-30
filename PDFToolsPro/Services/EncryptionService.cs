using System.IO;
using iText.Kernel.Pdf;
using PDFToolsPro.Models;

namespace PDFToolsPro.Services;

public class EncryptionService : IEncryptionService
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

    public async Task<(bool Success, string? ErrorMessage)> EncryptAsync(
        string inputPath,
        string outputPath,
        ProtectionSettings settings,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var reader = new PdfReader(inputPath);
                
                var writerProperties = new WriterProperties();

                // Calculate permissions
                int permissions = EncryptionConstants.ALLOW_SCREENREADERS;
                
                if (!settings.PreventPrinting)
                    permissions |= EncryptionConstants.ALLOW_PRINTING | EncryptionConstants.ALLOW_DEGRADED_PRINTING;
                
                if (!settings.PreventCopying)
                    permissions |= EncryptionConstants.ALLOW_COPY;
                
                if (!settings.PreventEditing)
                    permissions |= EncryptionConstants.ALLOW_MODIFY_CONTENTS | 
                                   EncryptionConstants.ALLOW_MODIFY_ANNOTATIONS |
                                   EncryptionConstants.ALLOW_FILL_IN |
                                   EncryptionConstants.ALLOW_ASSEMBLY;

                // User password - required to OPEN the document (منع المشاهدة)
                byte[]? userPassword = null;
                if (settings.RequirePasswordToOpen && !string.IsNullOrEmpty(settings.UserPassword))
                {
                    userPassword = System.Text.Encoding.UTF8.GetBytes(settings.UserPassword);
                }
                    
                // Owner password - controls permissions
                var ownerPassword = System.Text.Encoding.UTF8.GetBytes(
                    !string.IsNullOrEmpty(settings.OwnerPassword) 
                        ? settings.OwnerPassword 
                        : settings.UserPassword);

                writerProperties.SetStandardEncryption(
                    userPassword,
                    ownerPassword,
                    permissions,
                    EncryptionConstants.ENCRYPTION_AES_256);

                using var writer = new PdfWriter(outputPath, writerProperties);
                using var srcDoc = new PdfDocument(reader);
                using var destDoc = new PdfDocument(writer);

                srcDoc.CopyPagesTo(1, srcDoc.GetNumberOfPages(), destDoc);

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

    public async Task<(bool Success, string? ErrorMessage)> DecryptAsync(
        string inputPath,
        string outputPath,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var readerProperties = new ReaderProperties();
                readerProperties.SetPassword(System.Text.Encoding.UTF8.GetBytes(password));

                using var reader = new PdfReader(inputPath, readerProperties);
                reader.SetUnethicalReading(true);

                using var writer = new PdfWriter(outputPath);
                using var srcDoc = new PdfDocument(reader);
                using var destDoc = new PdfDocument(writer);

                srcDoc.CopyPagesTo(1, srcDoc.GetNumberOfPages(), destDoc);

            }, cancellationToken);

            return (true, null);
        }
        catch (OperationCanceledException)
        {
            CleanupFile(outputPath);
            return (false, "Operation was cancelled");
        }
        catch (iText.Kernel.Exceptions.BadPasswordException)
        {
            CleanupFile(outputPath);
            return (false, "Invalid password");
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

