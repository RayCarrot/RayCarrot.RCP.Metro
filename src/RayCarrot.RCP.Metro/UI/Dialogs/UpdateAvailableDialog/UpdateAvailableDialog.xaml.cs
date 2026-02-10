using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for UpdateAvailableDialog.xaml
/// </summary>
public partial class UpdateAvailableDialog : WindowContentControl
{
    public UpdateAvailableDialog(UpdateAvailableDialogViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
    }

    public UpdateAvailableDialogViewModel ViewModel { get; }

    public override bool IsResizable => true;

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Title = "New update available"; // TODO-LOC
        WindowInstance.Icon = GenericIconKind.Window_UpdateAvailable;
        WindowInstance.MinWidth = 600;
        WindowInstance.MinHeight = 400;
        WindowInstance.Width = 960;
        WindowInstance.Height = 640;
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        WindowInstance.Close();
    }

    private void ConfirmButton_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.RequestedInstallNewUpdate = true;
        WindowInstance.Close();
    }
}