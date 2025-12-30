using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDFToolsPro.Models;
using PDFToolsPro.Services;

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

    public CompressViewModel()
    {
        _compressorService = new PdfCompressorService();
        Files.CollectionChanged += (s, e) => ExecuteCompressCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteCompress))]
    private async Task ExecuteCompressAsync()
    {
        if (Files.Count == 0) return;

        _cts = new CancellationTokenSource();
        IsProcessing = true;
        Progress = 0;
        StatusMessage = Loc.Processing;

        var settings = new CompressionSettings { Level = SelectedLevel };
        var progress = new Progress<int>(p => Progress = p);

        long totalOriginal = 0;
        long totalNew = 0;
        int processedFiles = 0;

        try
        {
            foreach (var file in Files.ToList())
            {
                try
                {
                    var outputPath = GetOutputPath(file.FilePath);
                    if (string.IsNullOrEmpty(outputPath)) continue;

                    StatusMessage = $"{Loc.Processing}: {file.FileName}";

                    var result = await _compressorService.CompressAsync(
                        file.FilePath, 
                        outputPath, 
                        settings, 
                        progress, 
                        _cts.Token);

                    if (result.Success)
                    {
                        totalOriginal += result.OriginalSize;
                        totalNew += result.NewSize;
                        processedFiles++;
                    }
                    else
                    {
                        StatusMessage = $"{Loc.Error}: {result.ErrorMessage}";
                    }
                }
                catch (OperationCanceledException)
                {
                    StatusMessage = Loc.Cancel;
                    break;
                }
                catch (Exception ex)
                {
                    StatusMessage = $"{Loc.Error}: {ex.Message}";
                }
            }

            if (processedFiles > 0)
            {
                OriginalSize = FormatSize(totalOriginal);
                NewSize = FormatSize(totalNew);
                var reduction = (1 - (double)totalNew / totalOriginal) * 100;
                ReductionPercentage = $"{reduction:F1}%";
                var lastFile = Files.LastOrDefault();
                var outputName = lastFile != null ? GetOutputPath(lastFile.FilePath) : "";
                StatusMessage = $"{Loc.CompressCompleted}\n{Loc.SavedTo} {System.IO.Path.GetFileName(outputName)}";
                ShowSuccessMessage = true;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"{Loc.Error}: {ex.Message}";
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

    private string GetOutputPath(string inputPath)
    {
        var dir = System.IO.Path.GetDirectoryName(inputPath);
        var name = System.IO.Path.GetFileNameWithoutExtension(inputPath);
        var ext = System.IO.Path.GetExtension(inputPath);
        return System.IO.Path.Combine(dir ?? "", $"{name}_compressed{ext}");
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

