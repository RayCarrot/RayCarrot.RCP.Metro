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

    public static ImageSource EmulatorIconSource => (ImageSource)new ImageSourceConverter().ConvertFrom(EmulatorIconAsset.DosBox.GetAssetPath())!;
}