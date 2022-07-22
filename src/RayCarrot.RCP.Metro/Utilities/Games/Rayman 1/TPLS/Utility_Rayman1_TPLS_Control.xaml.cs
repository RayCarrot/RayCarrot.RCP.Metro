using System.Windows.Controls;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for Utility_Rayman1_TPLS_Control.xaml
/// </summary>
public partial class Utility_Rayman1_TPLS_Control : UserControl
{
    public Utility_Rayman1_TPLS_Control()
    {
        InitializeComponent();
    }

    public Utility_Rayman1_TPLS_ViewModel ViewModel => (Utility_Rayman1_TPLS_ViewModel)DataContext;

    private async void VersionSelection_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox c)
            return;

        var version = (Utility_Rayman1_TPLS_RaymanVersion)c.SelectedIndex;

        if (version == ViewModel.SelectedRaymanVersion)
            return;

        ViewModel.SelectedRaymanVersion = version;

        if (ViewModel.Data.Utility_TPLSData != null)
        {
            ViewModel.Data.Utility_TPLSData.RaymanVersion = ViewModel.SelectedRaymanVersion;
            await ViewModel.Data.Utility_TPLSData.UpdateConfigAsync();
        }
    }
}