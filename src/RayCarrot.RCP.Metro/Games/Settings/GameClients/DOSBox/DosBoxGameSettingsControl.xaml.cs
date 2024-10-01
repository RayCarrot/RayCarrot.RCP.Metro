using System.IO;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro.Games.Settings;

/// <summary>
/// Interaction logic for DosBoxGameSettingsControl.xaml
/// </summary>
public partial class DosBoxGameSettingsControl : UserControl
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public DosBoxGameSettingsControl()
    {
        InitializeComponent();
        MountPathBrowseBox.AllowedDriveTypes = new[] { DriveType.CDRom };
    }
}