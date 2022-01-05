using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BinarySerializer.OpenSpace;
using BinarySerializer.Ray1;
using BinarySerializer.UbiArt;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

public class Utility_Archives_ViewModel : BaseRCPViewModel, IDisposable
{
    #region Constructor

    public Utility_Archives_ViewModel()
    {
        Types = new ObservableCollection<Utility_Archives_TypeViewModel>()
        {
            new Utility_Archives_TypeViewModel(
                name: new ResourceLocString(resourcekey: nameof(Resources.Utilities_ArchiveExplorer_R1Header)),
                fileExtension: new FileExtension(".dat"),
                getManagerFunc: (data, mode) => new Ray1PCArchiveDataManager((Ray1Settings)data),
                modes: new Ray1GameMode[] 
                {
                    Ray1GameMode.RaymanEducational_PC,
                    Ray1GameMode.RaymanDesigner_PC,
                    Ray1GameMode.RaymanByHisFans_PC,
                    Ray1GameMode.Rayman60Levels_PC,
                }.Select(x => x.GetAttribute<Ray1GameModeInfoAttribute>()).
                Select(x => new Utility_Archives_ModeViewModel(
                    name: new ConstLocString(x.DisplayName), 
                    game: x.Game, 
                    data: new Ray1Settings(x.EngineVersion))).
                ToObservableCollection()),

            new Utility_Archives_TypeViewModel(
                name: new ResourceLocString(resourcekey: nameof(Resources.Utilities_ArchiveExplorer_CNTHeader)),
                fileExtension: new FileExtension(".cnt"),
                getManagerFunc: (data, mode) => new OpenSpaceCntArchiveDataManager((OpenSpaceSettings)data),
                modes: new OpenSpaceGameMode[] 
                {
                    OpenSpaceGameMode.Rayman2_PC,
                    OpenSpaceGameMode.Rayman2_Demo1_PC,
                    OpenSpaceGameMode.Rayman2_Demo2_PC,
                    OpenSpaceGameMode.RaymanM_PC,
                    OpenSpaceGameMode.RaymanArena_PC,
                    OpenSpaceGameMode.Rayman3_PC,
                    OpenSpaceGameMode.TonicTrouble_PC,
                    OpenSpaceGameMode.TonicTrouble_SE_PC,
                    OpenSpaceGameMode.DonaldDuck_PC,
                    OpenSpaceGameMode.PlaymobilHype_PC,
                }.Select(x => x.GetAttribute<OpenSpaceGameModeInfoAttribute>()).
                Select(x => new Utility_Archives_ModeViewModel(
                    name: new ConstLocString(x.DisplayName),
                    game: x.Game,
                    data: new OpenSpaceSettings(x.EngineVersion, x.Platform))).
                ToObservableCollection()),

            new Utility_Archives_TypeViewModel(
                name: new ResourceLocString(resourcekey: nameof(Resources.Utilities_ArchiveExplorer_IPKHeader)),
                fileExtension: new FileExtension(".ipk"),
                getManagerFunc: (data, mode) => new UbiArtIPKArchiveDataManager((UbiArtSettings)data, mode == Utility_Archives_TypeViewModel.ArchiveMode.Explorer 
                    ? UbiArtIPKArchiveConfigViewModel.FileCompressionMode.WasCompressed 
                    : UbiArtIPKArchiveConfigViewModel.FileCompressionMode.MatchesSetting),
                modes: new UbiArtGameMode[]
                {
                    UbiArtGameMode.RaymanOrigins_PC,
                    UbiArtGameMode.RaymanOrigins_PS3,
                    UbiArtGameMode.RaymanOrigins_Xbox360,
                    UbiArtGameMode.RaymanOrigins_Wii,
                    UbiArtGameMode.RaymanOrigins_PSVita,
                    UbiArtGameMode.RaymanLegends_PC,
                    UbiArtGameMode.RaymanLegends_Xbox360,
                    UbiArtGameMode.RaymanLegends_WiiU,
                    UbiArtGameMode.RaymanLegends_PSVita,
                    UbiArtGameMode.RaymanLegends_PS4,
                    UbiArtGameMode.RaymanLegends_Switch,
                    UbiArtGameMode.RaymanAdventures_Android,
                    UbiArtGameMode.RaymanAdventures_iOS,
                    UbiArtGameMode.RaymanMini_Mac,
                    UbiArtGameMode.JustDance_2017_WiiU,
                    UbiArtGameMode.ChildOfLight_PC,
                    UbiArtGameMode.ChildOfLight_PSVita,
                    UbiArtGameMode.ValiantHearts_Android,
                    UbiArtGameMode.GravityFalls_3DS,
                }.Select(x => x.GetAttribute<UbiArtGameModeInfoAttribute>()).
                Select(x => new Utility_Archives_ModeViewModel(
                    name: new ConstLocString(x.DisplayName),
                    game: x.Game,
                    data: new UbiArtSettings(x.UbiArtGame, x.Platform))).
                ToObservableCollection()),
        };
        SelectedType = Types.First();

        OpenArchiveCommand = new AsyncRelayCommand(OpenArchiveExplorerAsync);
        CreateArchiveCommand = new AsyncRelayCommand(CreateArchiveAsync);
    }

    #endregion

    #region Commands

    public ICommand OpenArchiveCommand { get; }
    public ICommand CreateArchiveCommand { get; }

    #endregion

    #region Public Properties

    public ObservableCollection<Utility_Archives_TypeViewModel> Types { get; }
    public Utility_Archives_TypeViewModel SelectedType { get; set; }

    #endregion

    #region Public Methods

    public async Task OpenArchiveExplorerAsync()
    {
        // Allow the user to select the files
        FileBrowserResult fileResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
        {
            Title = Resources.Utilities_ArchiveExplorer_FileSelectionHeader,
            DefaultDirectory = SelectedType.SelectedMode.Game?.GetInstallDir(false).FullPath,
            ExtensionFilter = SelectedType.FileExtension.GetFileFilterItem.ToString(),
            MultiSelection = true,
        });

        if (fileResult.CanceledByUser)
            return;

        // Get the manager
        using IArchiveDataManager manager = SelectedType.GetManager(Utility_Archives_TypeViewModel.ArchiveMode.Explorer);
        
        // Show the Archive Explorer
        await Services.UI.ShowArchiveExplorerAsync(manager, fileResult.SelectedFiles.ToArray());
    }

    public async Task CreateArchiveAsync()
    {
        // Get the manager
        using IArchiveDataManager manager = SelectedType.GetManager(Utility_Archives_TypeViewModel.ArchiveMode.Explorer);

        // Show the Archive Creator
        await Services.UI.ShowArchiveCreatorAsync(manager);
    }

    public void Dispose()
    {
        Types.DisposeAll();
    }

    #endregion
}