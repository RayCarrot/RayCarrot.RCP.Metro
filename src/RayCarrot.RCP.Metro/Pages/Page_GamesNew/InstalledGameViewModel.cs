using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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

        // TODO-14: Don't hard-code WPF paths like this as they're hard to find when reorganizing solution
        string bannerFileName = gameInstallation.GameDescriptor.Banner.GetAttribute<ImageFileAttribute>()?.FileName ?? "Default.png";
        GameBannerImageSource = $"{AppViewModel.WPFApplicationBasePath}Img/GameBanners/{bannerFileName}";

        GamePanels = new ObservableCollection<GamePanelViewModel>();
        AddGamePanels();
    }

    public GameInstallation GameInstallation { get; }
    public string DisplayName { get; }
    public PackIconMaterialKind IconKind { get; } // TODO-UPDATE: Use GenericIconKind

    public string IconSource => GameInstallation.GameDescriptor.IconSource;
    public bool IsDemo => GameInstallation.GameDescriptor.IsDemo;
    public string GameBannerImageSource { get; }

    public ObservableCollection<GamePanelViewModel> GamePanels { get; }

    private void AddGamePanels()
    {
        GameDescriptor gameDescriptor = GameInstallation.GameDescriptor;

        if (gameDescriptor.AllowPatching)
            GamePanels.Add(new PatcherGamePanelViewModel());
        
        if (gameDescriptor.HasArchives)
            GamePanels.Add(new ArchiveGamePanelViewModel());

        GamePanels.Add(new ProgressionGamePanelViewModel());
        GamePanels.Add(new LinksGamePanelViewModel());
    }

    public Task LoadAsync()
    {
        return Task.WhenAll(GamePanels.Select(x => x.LoadAsync(GameInstallation)));
    }
}