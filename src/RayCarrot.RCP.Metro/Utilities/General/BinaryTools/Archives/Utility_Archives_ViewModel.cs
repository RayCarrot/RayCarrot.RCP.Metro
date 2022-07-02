using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using NLog;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.CPA;
using RayCarrot.RCP.Metro.Archive.Ray1;
using RayCarrot.RCP.Metro.Archive.UbiArt;

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
                getManagerFunc: (data, mode) => new Ray1PCArchiveDataManager(data.GetAttribute<Ray1GameModeInfoAttribute>().GetSettings()),
                modes: new EnumSelectionViewModel<Enum>(Ray1GameMode.RaymanDesigner_PC, new Enum[]
                {
                    Ray1GameMode.RaymanEducational_PC,
                    Ray1GameMode.RaymanDesigner_PC,
                    Ray1GameMode.RaymanByHisFans_PC,
                    Ray1GameMode.Rayman60Levels_PC,
                })),

            new Utility_Archives_TypeViewModel(
                name: new ResourceLocString(resourcekey: nameof(Resources.Utilities_ArchiveExplorer_CNTHeader)),
                fileExtension: new FileExtension(".cnt"),
                getManagerFunc: (data, mode) => new CPACntArchiveDataManager(data.GetAttribute<CPAGameModeInfoAttribute>().GetSettings()),
                modes: new EnumSelectionViewModel<Enum>(CPAGameMode.Rayman2_PC, new Enum[]
                {
                    CPAGameMode.Rayman2_PC,
                    CPAGameMode.Rayman2_Demo1_PC,
                    CPAGameMode.Rayman2_Demo2_PC,
                    CPAGameMode.RaymanM_PC,
                    CPAGameMode.RaymanArena_PC,
                    CPAGameMode.Rayman3_PC,
                    CPAGameMode.TonicTrouble_PC,
                    CPAGameMode.TonicTrouble_SE_PC,
                    CPAGameMode.DonaldDuck_PC,
                    CPAGameMode.PlaymobilHype_PC,
                })),

            new Utility_Archives_TypeViewModel(
                name: new ResourceLocString(resourcekey: nameof(Resources.Utilities_ArchiveExplorer_IPKHeader)),
                fileExtension: new FileExtension(".ipk"),
                getManagerFunc: (data, mode) => new UbiArtIPKArchiveDataManager(data.GetAttribute<UbiArtGameModeInfoAttribute>().GetSettings(), mode == Utility_Archives_TypeViewModel.ArchiveMode.Explorer 
                    ? UbiArtIPKArchiveConfigViewModel.FileCompressionMode.WasCompressed 
                    : UbiArtIPKArchiveConfigViewModel.FileCompressionMode.MatchesSetting),
                modes: new EnumSelectionViewModel<Enum>(UbiArtGameMode.RaymanOrigins_PC, new Enum[]
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
                })),
        };
        SelectedType = Types.First();

        OpenArchiveCommand = new AsyncRelayCommand(OpenArchiveExplorerAsync);
        CreateArchiveCommand = new AsyncRelayCommand(CreateArchiveAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
            DefaultDirectory = SelectedType.Modes.SelectedValue.GetAttribute<GameModeBaseAttribute>()?.Game?.GetInstallDir(false).FullPath,
            ExtensionFilter = SelectedType.FileExtension.GetFileFilterItem.ToString(),
            MultiSelection = true,
        });

        if (fileResult.CanceledByUser)
            return;

        // Get the manager
        using IArchiveDataManager manager = SelectedType.GetManager(Utility_Archives_TypeViewModel.ArchiveMode.Explorer);

        try
        {
            // Show the Archive Explorer
            await Services.UI.ShowArchiveExplorerAsync(manager, fileResult.SelectedFiles.ToArray());
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Archive explorer");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Archive_CriticalError);
        }
    }

    public async Task CreateArchiveAsync()
    {
        // Get the manager
        using IArchiveDataManager manager = SelectedType.GetManager(Utility_Archives_TypeViewModel.ArchiveMode.Creator);

        // Show the Archive Creator
        await Services.UI.ShowArchiveCreatorAsync(manager);
    }

    public void Dispose()
    {
        Types.DisposeAll();
    }

    #endregion
}