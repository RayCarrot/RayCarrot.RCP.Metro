using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.OptionsDialog;

namespace RayCarrot.RCP.Metro.Games.Clients.DosBox;

public sealed class DosBoxGameClientDescriptor : EmulatorGameClientDescriptor
{
    #region Public Properties

    public override string GameClientId => "DOSBox";
    public override GamePlatform[] SupportedPlatforms => new[] { GamePlatform.MsDos };
    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.GameType_DosBox));
    public override GameClientIconAsset Icon => GameClientIconAsset.DosBox;

    #endregion

    #region Private Methods

    /// <summary>
    /// Gets the DOSBox configuration file path for the auto config file
    /// </summary>
    /// <returns>The file path</returns>
    private static FileSystemPath GetGameConfigFile(GameInstallation gameInstallation) =>
        AppFilePaths.UserDataBaseDir + "Clients" + "DOSBox" + (gameInstallation.InstallationId + ".ini");

    private static void CreateConfigFile(GameInstallation gameInstallation, GameClientInstallation gameClientInstallation) => 
        new AutoConfigManager(GetGameConfigFile(gameInstallation)).Create(gameInstallation);

    #endregion

    #region Public Methods

    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new OnGameClientAttachedComponent(CreateConfigFile));
        builder.Register<LaunchGameComponent>(new DosBoxLaunchGameComponent(this));
        
        // Add the RCP config file
        builder.Register(new DosBoxConfigFileComponent(GetGameConfigFile));

        // Client config page
        builder.Register(new GameOptionsDialogPageComponent(
            objFactory: x => new DosBoxGameConfigViewModel(
                gameInstallation: x, 
                gameClientInstallation: Services.GameClients.GetRequiredAttachedGameClient(x), 
                configFilePath: GetGameConfigFile(x)),
            isAvailableFunc: _ => true,
            // The id depends on the client as that determines the content
            getInstanceIdFunc: x => $"ClientConfig_{Services.GameClients.GetRequiredAttachedGameClient(x).InstallationId}"));
    }

    public override GameClientOptionsViewModel GetGameClientOptionsViewModel(GameClientInstallation gameClientInstallation) =>
        new DosBoxGameClientOptionsViewModel(gameClientInstallation, this);

    #endregion
}