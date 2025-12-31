using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PDFToolsPro.ViewModels;

namespace PDFToolsPro.Views;

public partial class SplitView : UserControl
{
    public SplitView()
    {
        InitializeComponent();
    }

    private void OnFileDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (DataContext is SplitViewModel vm)
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





