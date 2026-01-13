using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDFToolsPro.Helpers;

namespace PDFToolsPro.ViewModels;

public enum NavigationPage
{
    Compress,
    Merge,
    Split,
    Watermark,
    Protect,
    About
}

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private NavigationPage _currentPage = NavigationPage.Compress;

    [ObservableProperty]
    private object? _currentViewModel;

    [ObservableProperty]
    private bool _isArabic = true;

    public LocalizationHelper Loc => LocalizationHelper.Instance;

    public CompressViewModel CompressViewModel { get; } = new();
    public MergeViewModel MergeViewModel { get; } = new();
    public SplitViewModel SplitViewModel { get; } = new();
    public WatermarkViewModel WatermarkViewModel { get; } = new();
    public ProtectViewModel ProtectViewModel { get; } = new();
    public AboutViewModel AboutViewModel { get; } = new();

    public MainViewModel()
    {
        CurrentViewModel = CompressViewModel;
    }

    [RelayCommand]
    private void Navigate(NavigationPage page)
    {
        CurrentPage = page;
        CurrentViewModel = page switch
        {
            NavigationPage.Compress => CompressViewModel,
            NavigationPage.Merge => MergeViewModel,
            NavigationPage.Split => SplitViewModel,
            NavigationPage.Watermark => WatermarkViewModel,
            NavigationPage.Protect => ProtectViewModel,
            NavigationPage.About => AboutViewModel,
            _ => CompressViewModel
        };
    }

    [RelayCommand]
    private void SwitchLanguage()
    {
        Loc.SwitchLanguage();
        IsArabic = Loc.IsArabic;
    }
}






