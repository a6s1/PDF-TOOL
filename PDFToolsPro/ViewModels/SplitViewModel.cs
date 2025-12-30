using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDFToolsPro.Models;
using PDFToolsPro.Services;

namespace PDFToolsPro.ViewModels;

public enum SplitMode
{
    Range,
    EachPage,
    Extract
}

public partial class SplitViewModel : ViewModelBase
{
    private readonly IPdfSplitterService _splitterService;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private SplitMode _selectedMode = SplitMode.Range;

    [ObservableProperty]
    private int _startPage = 1;

    [ObservableProperty]
    private int _endPage = 1;

    [ObservableProperty]
    private string _pagesToExtract = string.Empty;

    [ObservableProperty]
    private int _totalPages;

    [ObservableProperty]
    private string _outputPath = string.Empty;

    public SplitViewModel()
    {
        _splitterService = new PdfSplitterService();
        Files.CollectionChanged += (s, e) => ExecuteSplitCommand.NotifyCanExecuteChanged();
    }

    protected override void SelectFiles()
    {
        base.SelectFiles();
        UpdatePageCount();
    }

    public override void HandleFileDrop(string[] files)
    {
        Files.Clear();
        if (files.Length > 0 && files[0].EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            Files.Add(new PdfFileInfo(files[0]));
            UpdatePageCount();
        }
    }

    private async void UpdatePageCount()
    {
        if (Files.Count > 0)
        {
            try
            {
                TotalPages = await _splitterService.GetPageCountAsync(Files[0].FilePath);
                EndPage = TotalPages;
            }
            catch
            {
                TotalPages = 0;
            }
        }
        else
        {
            TotalPages = 0;
        }
    }

    [RelayCommand]
    private void BrowseOutput()
    {
        if (SelectedMode == SplitMode.EachPage)
        {
            OutputPath = GetFolderPath();
        }
        else
        {
            OutputPath = GetSaveFilePath("split.pdf");
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteSplit))]
    private async Task ExecuteSplitAsync()
    {
        if (Files.Count == 0) return;

        var inputPath = Files[0].FilePath;
        _cts = new CancellationTokenSource();
        IsProcessing = true;
        Progress = 0;
        StatusMessage = Loc.Processing;

        try
        {
            (bool Success, string? ErrorMessage) result;

            switch (SelectedMode)
            {
                case SplitMode.Range:
                    var output = string.IsNullOrEmpty(OutputPath) 
                        ? GetSaveFilePath("split.pdf") 
                        : OutputPath;
                    if (string.IsNullOrEmpty(output)) return;
                    
                    result = await _splitterService.SplitByRangeAsync(
                        inputPath, output, StartPage, EndPage, _cts.Token);
                    break;

                case SplitMode.EachPage:
                    var folder = string.IsNullOrEmpty(OutputPath) 
                        ? GetFolderPath() 
                        : OutputPath;
                    if (string.IsNullOrEmpty(folder)) return;
                    
                    var progress = new Progress<int>(p => Progress = p);
                    result = await _splitterService.SplitEachPageAsync(
                        inputPath, folder, progress, _cts.Token);
                    break;

                case SplitMode.Extract:
                    var extractOutput = string.IsNullOrEmpty(OutputPath) 
                        ? GetSaveFilePath("extracted.pdf") 
                        : OutputPath;
                    if (string.IsNullOrEmpty(extractOutput)) return;
                    
                    var pages = ParsePageNumbers(PagesToExtract);
                    result = await _splitterService.ExtractPagesAsync(
                        inputPath, extractOutput, pages, _cts.Token);
                    break;

                default:
                    result = (false, "Invalid mode");
                    break;
            }

            if (result.Success)
            {
                var outputName = SelectedMode == SplitMode.EachPage 
                    ? OutputPath 
                    : System.IO.Path.GetFileName(OutputPath);
                StatusMessage = $"{Loc.SplitCompleted}\n{Loc.SavedTo} {outputName}";
                Progress = 100;
                ShowSuccessMessage = true;
            }
            else
            {
                StatusMessage = $"{Loc.Error}: {result.ErrorMessage}";
                Progress = 0;
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = Loc.Cancel;
        }
        finally
        {
            IsProcessing = false;
            _cts = null;
        }
    }

    private bool CanExecuteSplit() => Files.Count > 0 && !IsProcessing;

    [RelayCommand]
    private void CancelOperation()
    {
        _cts?.Cancel();
    }

    private int[] ParsePageNumbers(string input)
    {
        var pages = new List<int>();
        var parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (trimmed.Contains('-'))
            {
                var range = trimmed.Split('-');
                if (range.Length == 2 && 
                    int.TryParse(range[0].Trim(), out int start) && 
                    int.TryParse(range[1].Trim(), out int end))
                {
                    for (int i = start; i <= end; i++)
                    {
                        pages.Add(i);
                    }
                }
            }
            else if (int.TryParse(trimmed, out int page))
            {
                pages.Add(page);
            }
        }

        return pages.ToArray();
    }
}



