using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Design time data for XAML files
/// </summary>
public static class DesignData
{
    public static DriveSelectionViewModel DriveSelectionViewModel
    {
        get
        {
            var vm = new DriveSelectionViewModel();

            _ = vm.RefreshAsync();

            return vm;
        }
    }

    public static ImageSource GameIconSource => (ImageSource)new ImageSourceConverter().ConvertFrom(GameIconAsset.Rayman2.GetAssetPath())!;

    public static ImageSource GamePlatformIconSource => (ImageSource)new ImageSourceConverter().ConvertFrom(GamePlatformIconAsset.Win32.GetAssetPath())!;

    public static ImageSource GameClientIconSource => (ImageSource)new ImageSourceConverter().ConvertFrom(GameClientIconAsset.DosBox.GetAssetPath())!;

    public static GameInstallation GameInstallation => new(new GameDescriptor_Rayman2_Win32(), FileSystemPath.EmptyPath);
}