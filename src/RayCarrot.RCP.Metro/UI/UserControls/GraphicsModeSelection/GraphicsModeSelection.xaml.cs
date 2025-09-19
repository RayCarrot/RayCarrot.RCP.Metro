using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for GraphicsModeSelection.xaml
/// </summary>
public partial class GraphicsModeSelection : UserControl
{
    public GraphicsModeSelection()
    {
        InitializeComponent();

        // Hack to force left-to-right for textbox portion of the combobox
        ModesComboBox.ApplyTemplate();
        ((TextBox)ModesComboBox.Template.FindName("PART_EditableTextBox", ModesComboBox)).FlowDirection = FlowDirection.LeftToRight;
    }
}