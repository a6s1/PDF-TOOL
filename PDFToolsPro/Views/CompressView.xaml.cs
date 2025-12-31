using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PDFToolsPro.ViewModels;

namespace PDFToolsPro.Views;

public partial class CompressView : UserControl
{
    public CompressView()
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
}





