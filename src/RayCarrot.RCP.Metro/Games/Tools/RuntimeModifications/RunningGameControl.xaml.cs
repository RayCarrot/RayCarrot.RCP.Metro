using System.Windows.Controls;

namespace RayCarrot.RCP.Metro.Games.Tools.RuntimeModifications;

/// <summary>
/// Interaction logic for RunningGameControl.xaml
/// </summary>
public partial class RunningGameControl : UserControl
{
    public RunningGameControl()
    {
        InitializeComponent();
    }

    private void TabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is RunningGameViewModel vm && sender is TabControl tc)
            vm.LogEnabled = tc.SelectedIndex == 1;
    }
}