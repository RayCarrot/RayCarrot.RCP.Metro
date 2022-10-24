using System.Windows.Controls;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for Ray1MinigamesOptions.xaml
/// </summary>
public partial class GameOptions_Ray1Minigames_Controls : UserControl
{
    public GameOptions_Ray1Minigames_Controls(GameOptions_Ray1Minigames_ViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}