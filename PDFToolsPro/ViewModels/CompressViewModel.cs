using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PDFToolsPro.Models;
using PDFToolsPro.Services;
using System.IO;

namespace PDFToolsPro.ViewModels;

public partial class CompressViewModel : ViewModelBase
{
    private readonly IPdfCompressorService _compressorService;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private CompressionLevel _selectedLevel = CompressionLevel.Medium;

    [ObservableProperty]
    private string _originalSize = "-";

    [ObservableProperty]
    private string _newSize = "-";

    [ObservableProperty]
    private string _reductionPercentage = "-";

    [ObservableProperty]
    private string _outputFolder = string.Empty;

    [ObservableProperty]
    private bool _saveInSameFolder = true;

    public CompressViewModel()
    {
        _compressorService = new PdfCompressorService();
        Files.CollectionChanged += (s, e) => ExecuteCompressCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void BrowseOutputFolder()
    {
        var dialog = new OpenFolderDialog
        {
            Title = Loc.IsArabic ? "اختر مجلد الحفظ" : "Select Output Folder"
        };
        
        if (dialog.ShowDialog() == true)
        {
            OutputFolder = dialog.FolderName;
            SaveInSameFolder = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteCompress))]
    private async Task ExecuteCompressAsync()
    {
        if (Files.Count == 0) return;

        _cts = new CancellationTokenSource();
        IsProcessing = true;
        Progress = 0;
        ShowSuccessMessage = false;
        
        // Reset stats
        OriginalSize = "-";
        NewSize = "-";
        ReductionPercentage = "-";

        var settings = new CompressionSettings { Level = SelectedLevel };

        long totalOriginal = 0;
        long totalNew = 0;
        int processedFiles = 0;
        int totalFiles = Files.Count;
        var compressedFilesList = new List<string>();

        try
        {
            for (int i = 0; i < Files.Count; i++)
            {
                var file = Files[i];
                
                try
                {
                    // Calculate output path
                    string outputPath;
                    if (SaveInSameFolder || string.IsNullOrEmpty(OutputFolder))
                    {
                        // Save in same folder with _compressed suffix
                        var dir = Path.GetDirectoryName(file.FilePath) ?? "";
                        var name = Path.GetFileNameWithoutExtension(file.FilePath);
                        outputPath = Path.Combine(dir, $"{name}_compressed.pdf");
                    }
                    else
                    {
                        // Save in selected folder
                        var name = Path.GetFileNameWithoutExtension(file.FilePath);
                        outputPath = Path.Combine(OutputFolder, $"{name}_compressed.pdf");
                    }

                    StatusMessage = Loc.IsArabic 
                        ? $"جاري ضغط ({i + 1}/{totalFiles}): {file.FileName}" 
                        : $"Compressing ({i + 1}/{totalFiles}): {file.FileName}";

                    // Progress for this file (scaled to overall progress)
                    var fileProgress = new Progress<int>(p => 
                    {
                        int overallProgress = (int)((i * 100.0 + p) / totalFiles);
                        Progress = overallProgress;
                    });

                    var result = await _compressorService.CompressAsync(
                        file.FilePath, 
                        outputPath, 
                        settings, 
                        fileProgress, 
                        _cts.Token);

                    if (result.Success)
                    {
                        totalOriginal += result.OriginalSize;
                        totalNew += result.NewSize;
                        processedFiles++;
                        compressedFilesList.Add(Path.GetFileName(outputPath));
                    }
                    else
                    {
                        StatusMessage = $"❌ {file.FileName}: {result.ErrorMessage}";
                    }
                }
                catch (OperationCanceledException)
                {
                    StatusMessage = Loc.Cancel;
                    break;
                }
                catch (Exception ex)
                {
                    StatusMessage = $"❌ {file.FileName}: {ex.Message}";
                }
            }

            Progress = 100;

            if (processedFiles > 0)
            {
                OriginalSize = FormatSize(totalOriginal);
                NewSize = FormatSize(totalNew);
                var reduction = totalOriginal > 0 ? (1 - (double)totalNew / totalOriginal) * 100 : 0;
                ReductionPercentage = $"{reduction:F1}%";
                
                // Build success message
                string saveLocation = SaveInSameFolder || string.IsNullOrEmpty(OutputFolder) 
                    ? (Loc.IsArabic ? "نفس المجلد" : "Same folder") 
                    : OutputFolder;

                if (processedFiles == 1)
                {
                    StatusMessage = Loc.IsArabic 
                        ? $"✅ تم ضغط الملف بنجاح!\n📁 {compressedFilesList[0]}\n📂 {saveLocation}"
                        : $"✅ File compressed successfully!\n📁 {compressedFilesList[0]}\n📂 {saveLocation}";
                }
                else
                {
                    StatusMessage = Loc.IsArabic 
                        ? $"✅ تم ضغط {processedFiles} ملفات بنجاح!\n📂 {saveLocation}\n📊 توفير: {ReductionPercentage}"
                        : $"✅ {processedFiles} files compressed successfully!\n📂 {saveLocation}\n📊 Saved: {ReductionPercentage}";
                }
                
                ShowSuccessMessage = true;

                // Open output folder
                try
                {
                    string folderToOpen = SaveInSameFolder || string.IsNullOrEmpty(OutputFolder)
                        ? Path.GetDirectoryName(Files[0].FilePath) ?? ""
                        : OutputFolder;
                    
                    if (!string.IsNullOrEmpty(folderToOpen))
                    {
                        System.Diagnostics.Process.Start("explorer.exe", folderToOpen);
                    }
                }
                catch { }
            }
            else
            {
                StatusMessage = Loc.IsArabic 
                    ? "❌ لم يتم ضغط أي ملف" 
                    : "❌ No files were compressed";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ {Loc.Error}: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
            _cts = null;
        }
    }

    private bool CanExecuteCompress() => Files.Count > 0 && !IsProcessing;

    [RelayCommand]
    private void CancelOperation()
    {
        _cts?.Cancel();
    }

    private string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F2} MB";
    }

    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(Files) || e.PropertyName == nameof(IsProcessing))
        {
            ExecuteCompressCommand.NotifyCanExecuteChanged();
        }
    }
}
