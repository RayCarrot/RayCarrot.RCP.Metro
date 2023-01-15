using System.Windows;
using System.Windows.Controls;
using GongSolutions.Wpf.DragDrop;
using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

namespace RayCarrot.RCP.Metro.Games.Clients.DosBox;

/// <summary>
/// Interaction logic for DosBoxGameClientOptionsControl.xaml
/// </summary>
public partial class DosBoxGameClientOptionsControl : UserControl
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public DosBoxGameClientOptionsControl()
    {
        InitializeComponent();
        Loaded += DosBoxGameClientOptionsControl_Loaded;
    }

    private void DosBoxGameClientOptionsControl_Loaded(object sender, RoutedEventArgs e)
    {
        DragDrop.SetDropHandler(ConfigFilesListBox, new DropHandler(ViewModel));
        Loaded -= DosBoxGameClientOptionsControl_Loaded;
    }

    public DosBoxGameClientOptionsViewModel ViewModel => (DosBoxGameClientOptionsViewModel)DataContext;

    private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        DosBoxGameClientOptionsViewModel viewModel = ViewModel;

        foreach (FileSystemPath path in ConfigFilesListBox.SelectedItems.Cast<FileSystemPath>().ToList())
            viewModel.ConfigFiles.Remove(path);

        viewModel.ApplyChanges();
    }

    private class DropHandler : DefaultDropHandler
    {
        public DropHandler(DosBoxGameClientOptionsViewModel viewModel) => ViewModel = viewModel;

        private DosBoxGameClientOptionsViewModel ViewModel { get; }

        public override void Drop(IDropInfo dropInfo)
        {
            base.Drop(dropInfo);
            ViewModel.ApplyChanges();
        }
    }
}