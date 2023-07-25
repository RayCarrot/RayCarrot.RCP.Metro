using System.Windows;

namespace RayCarrot.RCP.Metro;

// TODO: Don't show if user hasn't used RCP before. Retake screenshots.

/// <summary>
/// Interaction logic for AnniversaryUpdateDialog.xaml
/// </summary>
public partial class AnniversaryUpdateDialog : WindowContentControl
{
    public AnniversaryUpdateDialog()
    {
        InitializeComponent();
    }

    public override bool IsResizable => true;

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Title = Metro.Resources.AnniversaryUpdate_Title;
        WindowInstance.Icon = GenericIconKind.Window_AnniversaryUpdate;
        WindowInstance.MinWidth = 900;
        WindowInstance.MinHeight = 600;
        WindowInstance.Width = 900;
        WindowInstance.Height = 600;
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        WindowInstance.Close();
    }
}