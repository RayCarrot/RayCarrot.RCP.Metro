using RayCarrot.RCP.Metro.Games.Clients.Data;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Finder;
using RayCarrot.RCP.Metro.Games.OptionsDialog;

namespace RayCarrot.RCP.Metro.Games.Clients.DosBox;

public sealed class DosBoxGameClientDescriptor : EmulatorGameClientDescriptor
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public override string GameClientId => "DOSBox";
    public override bool InstallationRequiresFile => true;
    public override GamePlatform[] SupportedPlatforms => new[] { GamePlatform.MsDos };
    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.GameType_DosBox));
    public override GameClientIconAsset Icon => GameClientIconAsset.DosBox;

    #endregion

    #region Private Methods

    private static void CreateConfigFile(GameInstallation gameInstallation, GameClientInstallation gameClientInstallation)
    {
        var configFile = gameInstallation.GetRequiredComponent<DosBoxConfigFileComponent, AutoDosBoxConfigFileComponent>().CreateObject();
        new AutoConfigManager(configFile).Create(gameInstallation);
    }

    #endregion

    #region Public Methods

    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new OnGameClientAttachedComponent(CreateConfigFile));
        builder.Register<LaunchGameComponent>(new DosBoxLaunchGameComponent(this));
        
        // Add the RCP config file
        builder.Register<DosBoxConfigFileComponent, AutoDosBoxConfigFileComponent>();

        // Client config page
        builder.Register(new GameOptionsDialogPageComponent(
            objFactory: x => new DosBoxGameConfigViewModel(
                gameInstallation: x, 
                gameClientInstallation: Services.GameClients.GetRequiredAttachedGameClient(x), 
                configFilePath: x.GetRequiredComponent<DosBoxConfigFileComponent, AutoDosBoxConfigFileComponent>().CreateObject()),
            isAvailableFunc: _ => true,
            // The id depends on the client as that determines the content
            getInstanceIdFunc: x => $"ClientConfig_{Services.GameClients.GetRequiredAttachedGameClient(x).InstallationId}"));
    }

    public override GameClientOptionsViewModel GetGameClientOptionsViewModel(GameClientInstallation gameClientInstallation) =>
        new DosBoxGameClientOptionsViewModel(gameClientInstallation, this);

    public override async Task OnGameClientAddedAsync(GameClientInstallation gameClientInstallation)
    {
        await base.OnGameClientAddedAsync(gameClientInstallation);

        try
        {
            // Add the config file from Rayman Forever if found
            FileSystemPath raymanForeverConfigPath = gameClientInstallation.InstallLocation.Directory.Parent + "dosboxRayman.conf";

            if (raymanForeverConfigPath.FileExists)
                gameClientInstallation.ModifyObject<DosBoxConfigFilePaths>(GameClientDataKey.DosBox_ConfigFilePaths, 
                    x => x.FilePaths.Add(raymanForeverConfigPath));
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Finding DOSBox config file");
        }
    }

    public override FinderQuery[] GetFinderQueries()
    {
        const string fileName = "DOSBox.exe";

        static InstallLocation validateRaymanForeverLocation(InstallLocation location) => 
            InstallLocation.FromFilePath(location.Directory.Parent + "DosBox" + "DOSBox.exe");

        return new FinderQuery[]
        {
            // Find DOSBox in Rayman Forever installation
            new UninstallProgramFinderQuery("Rayman Forever") { ValidateLocationFunc = validateRaymanForeverLocation },
            new Win32ShortcutFinderQuery("Rayman Forever") { ValidateLocationFunc = validateRaymanForeverLocation },

            // Find standalone DOSBox installations
            new UninstallProgramFinderQuery("DOSBox") { FileName = fileName },
            new Win32ShortcutFinderQuery("DOSBox") { FileName = fileName },
        };
    }

    #endregion
}