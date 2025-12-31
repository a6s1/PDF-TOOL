using CommunityToolkit.Mvvm.ComponentModel;

namespace PDFToolsPro.Models;

public partial class PdfFileInfo : ObservableObject
{
    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private long _fileSize;

    [ObservableProperty]
    private int _pageCount;

    [ObservableProperty]
    private bool _isSelected;

    public string FileSizeFormatted
    {
        get
        {
            if (FileSize < 1024)
                return $"{FileSize} B";
            if (FileSize < 1024 * 1024)
                return $"{FileSize / 1024.0:F1} KB";
            return $"{FileSize / (1024.0 * 1024.0):F2} MB";
        }
    }

    public PdfFileInfo() { }

    public PdfFileInfo(string filePath)
    {
        FilePath = filePath;
        FileName = System.IO.Path.GetFileName(filePath);
        if (System.IO.File.Exists(filePath))
        {
            FileSize = new System.IO.FileInfo(filePath).Length;
        }
    }
}





