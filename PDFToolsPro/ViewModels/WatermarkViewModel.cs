using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PDFToolsPro.Models;
using PDFToolsPro.Services;

namespace PDFToolsPro.ViewModels;

public partial class WatermarkViewModel : ViewModelBase
{
    private readonly IWatermarkService _watermarkService;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private WatermarkType _watermarkType = WatermarkType.Text;

    [ObservableProperty]
    private string _watermarkText = "CONFIDENTIAL";

    [ObservableProperty]
    private string _imagePath = string.Empty;

    [ObservableProperty]
    private double _opacity = 0.3;

    [ObservableProperty]
    private double _angle = 45;

    [ObservableProperty]
    private WatermarkPosition _position = WatermarkPosition.Center;

    [ObservableProperty]
    private int _fontSize = 48;

    [ObservableProperty]
    private string _outputPath = string.Empty;

    public WatermarkViewModel()
    {
        _watermarkService = new WatermarkService();
        Files.CollectionChanged += (s, e) => ExecuteWatermarkCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void SelectImage()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
            Title = Loc.SelectImage
        };

        if (dialog.ShowDialog() == true)
        {
            ImagePath = dialog.FileName;
        }
    }

    [RelayCommand]
    private void BrowseOutput()
    {
        OutputPath = GetSaveFilePath("watermarked.pdf");
    }

    [RelayCommand(CanExecute = nameof(CanExecuteWatermark))]
    private async Task ExecuteWatermarkAsync()
    {
        if (Files.Count == 0) return;

        _cts = new CancellationTokenSource();
        IsProcessing = true;
        Progress = 0;
        StatusMessage = Loc.Processing;

        var settings = new WatermarkSettings
        {
            Type = WatermarkType,
            Text = WatermarkText,
            ImagePath = ImagePath,
            Opacity = (float)Opacity,
            Angle = (float)Angle,
            Position = Position,
            FontSize = FontSize
        };

        var progress = new Progress<int>(p => Progress = p);
        int successCount = 0;

        foreach (var file in Files.ToList())
        {
            try
            {
                var output = GetOutputPath(file.FilePath);
                var result = await _watermarkService.AddWatermarkAsync(
                    file.FilePath, output, settings, progress, _cts.Token);

                if (result.Success)
                {
                    successCount++;
                }
                else
                {
                    StatusMessage = $"{Loc.Error}: {result.ErrorMessage}";
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        if (successCount > 0)
        {
            var lastFile = Files.LastOrDefault();
            var outputName = lastFile != null ? System.IO.Path.GetFileName(GetOutputPath(lastFile.FilePath)) : "";
            StatusMessage = $"{Loc.WatermarkCompleted}\n{Loc.SavedTo} {outputName}";
            ShowSuccessMessage = true;
        }
        IsProcessing = false;
        _cts = null;
    }

    private bool CanExecuteWatermark() => Files.Count > 0 && !IsProcessing &&
        (WatermarkType == WatermarkType.Text ? !string.IsNullOrEmpty(WatermarkText) : !string.IsNullOrEmpty(ImagePath));

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
        return System.IO.Path.Combine(dir ?? "", $"{name}_watermarked{ext}");
    }
}



