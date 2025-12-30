using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PDFToolsPro.Helpers;
using PDFToolsPro.Models;
using System.Collections.ObjectModel;

namespace PDFToolsPro.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<PdfFileInfo> _files = new();

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private int _progress;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _showSuccessMessage;

    public LocalizationHelper Loc => LocalizationHelper.Instance;

    // Explicit command properties
    public IRelayCommand SelectFilesCommand { get; }
    public IRelayCommand<PdfFileInfo> RemoveFileCommand { get; }
    public IRelayCommand ClearFilesCommand { get; }
    public IRelayCommand<PdfFileInfo> MoveUpCommand { get; }
    public IRelayCommand<PdfFileInfo> MoveDownCommand { get; }

    protected ViewModelBase()
    {
        SelectFilesCommand = new RelayCommand(SelectFilesInternal);
        RemoveFileCommand = new RelayCommand<PdfFileInfo>(RemoveFile);
        ClearFilesCommand = new RelayCommand(ClearFiles);
        MoveUpCommand = new RelayCommand<PdfFileInfo>(MoveUp);
        MoveDownCommand = new RelayCommand<PdfFileInfo>(MoveDown);
    }

    private void SelectFilesInternal()
    {
        SelectFiles();
    }

    protected virtual void SelectFiles()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "PDF Files (*.pdf)|*.pdf",
            Multiselect = true,
            Title = Loc.SelectFiles
        };

        if (dialog.ShowDialog() == true)
        {
            foreach (var file in dialog.FileNames)
            {
                if (!Files.Any(f => f.FilePath == file))
                {
                    Files.Add(new PdfFileInfo(file));
                }
            }
        }
    }

    protected virtual void RemoveFile(PdfFileInfo? file)
    {
        if (file != null)
        {
            Files.Remove(file);
        }
    }

    protected virtual void ClearFiles()
    {
        Files.Clear();
    }

    protected virtual void MoveUp(PdfFileInfo? file)
    {
        if (file == null) return;
        var index = Files.IndexOf(file);
        if (index > 0)
        {
            Files.Move(index, index - 1);
        }
    }

    protected virtual void MoveDown(PdfFileInfo? file)
    {
        if (file == null) return;
        var index = Files.IndexOf(file);
        if (index < Files.Count - 1)
        {
            Files.Move(index, index + 1);
        }
    }

    public virtual void HandleFileDrop(string[] files)
    {
        foreach (var file in files)
        {
            if (file.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) && 
                !Files.Any(f => f.FilePath == file))
            {
                Files.Add(new PdfFileInfo(file));
            }
        }
    }

    protected string GetSaveFilePath(string defaultName = "output.pdf")
    {
        var dialog = new SaveFileDialog
        {
            Filter = "PDF Files (*.pdf)|*.pdf",
            FileName = defaultName,
            Title = Loc.OutputFile
        };

        return dialog.ShowDialog() == true ? dialog.FileName : string.Empty;
    }

    protected string GetFolderPath()
    {
        var dialog = new OpenFolderDialog
        {
            Title = Loc.OutputFile
        };

        return dialog.ShowDialog() == true 
            ? dialog.FolderName 
            : string.Empty;
    }
}
