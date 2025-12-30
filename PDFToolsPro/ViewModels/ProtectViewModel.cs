using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDFToolsPro.Models;
using PDFToolsPro.Services;

namespace PDFToolsPro.ViewModels;

public partial class ProtectViewModel : ViewModelBase
{
    private readonly IEncryptionService _encryptionService;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private bool _isEncryptMode = true;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private bool _requirePasswordToOpen = true;  // منع المشاهدة

    [ObservableProperty]
    private bool _preventPrinting;

    [ObservableProperty]
    private bool _preventCopying;

    [ObservableProperty]
    private bool _preventEditing;

    [ObservableProperty]
    private string _outputPath = string.Empty;

    public ProtectViewModel()
    {
        _encryptionService = new EncryptionService();
        Files.CollectionChanged += (s, e) => ExecuteProtectCommand.NotifyCanExecuteChanged();
    }

    partial void OnPasswordChanged(string value)
    {
        ExecuteProtectCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void BrowseOutput()
    {
        OutputPath = GetSaveFilePath(IsEncryptMode ? "protected.pdf" : "unprotected.pdf");
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProtect))]
    private async Task ExecuteProtectAsync()
    {
        if (Files.Count == 0) return;

        if (IsEncryptMode && Password != ConfirmPassword)
        {
            StatusMessage = Loc.IsArabic 
                ? "كلمتا المرور غير متطابقتين" 
                : "Passwords do not match";
            return;
        }

        _cts = new CancellationTokenSource();
        IsProcessing = true;
        Progress = 0;
        StatusMessage = Loc.Processing;

        int successCount = 0;

        foreach (var file in Files.ToList())
        {
            try
            {
                var output = GetOutputPath(file.FilePath);
                (bool Success, string? ErrorMessage) result;

                if (IsEncryptMode)
                {
                    var settings = new ProtectionSettings
                    {
                        UserPassword = Password,
                        OwnerPassword = Password,
                        RequirePasswordToOpen = RequirePasswordToOpen,
                        PreventPrinting = PreventPrinting,
                        PreventCopying = PreventCopying,
                        PreventEditing = PreventEditing
                    };

                    result = await _encryptionService.EncryptAsync(
                        file.FilePath, output, settings, _cts.Token);
                }
                else
                {
                    result = await _encryptionService.DecryptAsync(
                        file.FilePath, output, Password, _cts.Token);
                }

                if (result.Success)
                {
                    successCount++;
                }
                else
                {
                    StatusMessage = $"{Loc.Error}: {result.ErrorMessage}";
                }

                Progress = (int)((double)(Files.ToList().IndexOf(file) + 1) / Files.Count * 100);
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
            var completedMsg = IsEncryptMode ? Loc.ProtectCompleted : Loc.UnprotectCompleted;
            StatusMessage = $"{completedMsg}\n{Loc.SavedTo} {outputName}";
            ShowSuccessMessage = true;
        }
        IsProcessing = false;
        _cts = null;
    }

    private bool CanExecuteProtect() => Files.Count > 0 && !IsProcessing && !string.IsNullOrEmpty(Password);

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
        var suffix = IsEncryptMode ? "_protected" : "_unprotected";
        return System.IO.Path.Combine(dir ?? "", $"{name}{suffix}{ext}");
    }
}



