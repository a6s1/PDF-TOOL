using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDFToolsPro.Models;
using PDFToolsPro.Services;

namespace PDFToolsPro.ViewModels;

public partial class MergeViewModel : ViewModelBase
{
    private readonly IPdfMergerService _mergerService;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private bool _compressAfterMerge;

    [ObservableProperty]
    private CompressionLevel _compressionLevel = CompressionLevel.Medium;

    [ObservableProperty]
    private string _outputPath = string.Empty;

    public MergeViewModel()
    {
        _mergerService = new PdfMergerService();
        Files.CollectionChanged += (s, e) => ExecuteMergeCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void BrowseOutput()
    {
        OutputPath = GetSaveFilePath("merged.pdf");
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMerge))]
    private async Task ExecuteMergeAsync()
    {
        try
        {
            ShowSuccessMessage = false;
            
            if (Files.Count < 2) 
            {
                StatusMessage = Loc.IsArabic ? "يجب اختيار ملفين على الأقل" : "Select at least 2 files";
                return;
            }

            // Get save location
            var output = GetSaveFilePath("merged.pdf");
                
            if (string.IsNullOrEmpty(output)) 
            {
                StatusMessage = Loc.IsArabic ? "لم يتم اختيار مكان الحفظ" : "No save location selected";
                return;
            }

            _cts = new CancellationTokenSource();
            IsProcessing = true;
            Progress = 0;
            
            var fileCount = Files.Count;
            StatusMessage = Loc.IsArabic 
                ? $"جاري دمج {fileCount} ملفات..." 
                : $"Merging {fileCount} files...";

            var progress = new Progress<int>(p => 
            {
                Progress = p;
                if (CompressAfterMerge && p > 50)
                {
                    StatusMessage = Loc.IsArabic 
                        ? $"جاري الضغط... {p}%" 
                        : $"Compressing... {p}%";
                }
                else
                {
                    StatusMessage = Loc.IsArabic 
                        ? $"جاري الدمج... {p}%" 
                        : $"Merging... {p}%";
                }
            });
            
            var inputPaths = Files.Select(f => f.FilePath).ToList();

            var result = await _mergerService.MergeAsync(
                inputPaths, 
                output, 
                CompressAfterMerge,
                CompressionLevel,
                progress, 
                _cts.Token);

            if (result.Success)
            {
                if (System.IO.File.Exists(output))
                {
                    var fileInfo = new System.IO.FileInfo(output);
                    var sizeInMB = fileInfo.Length / (1024.0 * 1024.0);
                    
                    StatusMessage = Loc.IsArabic 
                        ? $"✅ تم الدمج بنجاح!\n📁 {System.IO.Path.GetFileName(output)}\n📂 {output}\n📊 الحجم: {sizeInMB:F2} MB"
                        : $"✅ Merge completed!\n📁 {System.IO.Path.GetFileName(output)}\n📂 {output}\n📊 Size: {sizeInMB:F2} MB";
                    
                    OutputPath = output;
                    ShowSuccessMessage = true;
                    Progress = 100;
                    
                    // Open folder with file selected
                    try
                    {
                        System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{output}\"");
                    }
                    catch { }
                }
                else
                {
                    StatusMessage = Loc.IsArabic 
                        ? "❌ خطأ: الملف لم يُنشأ" 
                        : "❌ Error: File was not created";
                }
            }
            else
            {
                StatusMessage = $"❌ {Loc.Error}: {result.ErrorMessage ?? "Unknown error"}";
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

    private bool CanExecuteMerge() => Files.Count >= 2 && !IsProcessing;

    [RelayCommand]
    private void CancelOperation()
    {
        _cts?.Cancel();
    }
}
