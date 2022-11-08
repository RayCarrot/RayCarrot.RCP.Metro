using System;
using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro;

public class InstalledGameViewModel : BaseViewModel
{
    public InstalledGameViewModel(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;
        DisplayName = gameInstallation.GameDescriptor.DisplayName;

        // TODO-UPDATE: Don't do this here
        IconKind = gameInstallation.GameDescriptor.Platform switch
        {
            GamePlatform.MSDOS => PackIconMaterialKind.DesktopClassic,
            GamePlatform.Win32 => PackIconMaterialKind.MicrosoftWindows,
            GamePlatform.Steam => PackIconMaterialKind.Steam,
            GamePlatform.WindowsPackage => PackIconMaterialKind.Package,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public GameInstallation GameInstallation { get; }
    public string DisplayName { get; }
    public PackIconMaterialKind IconKind { get; } // TODO-UPDATE: Use GenericIconKind
}