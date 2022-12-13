using System.IO;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro.Games.Emulators.DosBox;

/// <summary>
/// Interaction logic for DosBoxGameConfigControl.xaml
/// </summary>
public partial class DosBoxGameConfigControl : UserControl
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public DosBoxGameConfigControl()
    {
        InitializeComponent();
        MountPathBrowseBox.AllowedDriveTypes = new[] { DriveType.CDRom };
    }
}