using System.Windows.Controls;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for PrintStudioOptions.xaml
/// </summary>
public partial class GameOptions_PrintStudio_Control : UserControl
{
    public GameOptions_PrintStudio_Control(GameOptions_PrintStudio_ViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}