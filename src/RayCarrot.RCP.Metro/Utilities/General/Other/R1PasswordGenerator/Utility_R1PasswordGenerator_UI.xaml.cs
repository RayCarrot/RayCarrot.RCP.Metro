#nullable disable
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for Utility_R1PasswordGenerator_UI.xaml
/// </summary>
public partial class Utility_R1PasswordGenerator_UI : UserControl
{
    public Utility_R1PasswordGenerator_UI(Utility_R1PasswordGenerator_ViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
    }

    public Utility_R1PasswordGenerator_ViewModel ViewModel
    {
        get => (Utility_R1PasswordGenerator_ViewModel)DataContext;
        set => DataContext = value;
    }
}