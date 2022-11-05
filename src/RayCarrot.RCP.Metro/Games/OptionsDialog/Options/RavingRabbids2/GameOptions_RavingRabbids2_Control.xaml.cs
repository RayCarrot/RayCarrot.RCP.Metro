using System.Windows.Controls;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for RavingRabbids2Options.xaml
/// </summary>
public partial class GameOptions_RavingRabbids2_Control : UserControl
{
    public GameOptions_RavingRabbids2_Control(GameOptions_RavingRabbids2_ViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}