using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PDFToolsPro.ViewModels;

namespace PDFToolsPro.Views;

public partial class ProtectView : UserControl
{
    public ProtectView()
    {
        InitializeComponent();
    }

    private void OnFileDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (DataContext is ViewModelBase vm)
            {
                vm.HandleFileDrop(files);
            }
        }
    }

    private void OnDropZoneClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is ViewModelBase vm)
        {
            vm.SelectFilesCommand.Execute(null);
        }
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProtectViewModel vm)
        {
            vm.Password = PasswordBox.Password;
        }
    }

    private void OnConfirmPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProtectViewModel vm)
        {
            vm.ConfirmPassword = ConfirmPasswordBox.Password;
        }
    }
}



