using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for VersionHistoryDialog.xaml
/// </summary>
public partial class VersionHistoryDialog : WindowContentControl
{
    public VersionHistoryDialog()
    {
        InitializeComponent();
    }

    public override bool IsResizable => true;

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Title = Metro.Resources.AppNews_Header;
        WindowInstance.Icon = GenericIconKind.Window_AppNews;
        WindowInstance.MinWidth = 600;
        WindowInstance.MinHeight = 400;
        WindowInstance.Width = 960;
        WindowInstance.Height = 640;
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        WindowInstance.Close();
    }
}