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

    #endregion

    #region Public Methods

    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register<LaunchGameComponent>(new DosBoxLaunchGameComponent(this));
        
        // Add the RCP config file
        builder.Register(new DosBoxConfigFileComponent(GetGameConfigFile));

        // Client config page
        builder.Register(new GameOptionsDialogPageComponent(
            objFactory: x => new DosBoxGameConfigViewModel(
                gameInstallation: x, 
                gameClientInstallation: Services.GameClients.GetRequiredAttachedGameClient(x), 
                configFilePath: GetGameConfigFile(x)),
            isAvailableFunc: _ => true));
    }

    public override GameClientOptionsViewModel GetGameClientOptionsViewModel(GameClientInstallation gameClientInstallation) =>
        new DosBoxGameClientOptionsViewModel(gameClientInstallation, this);

    // TODO-14: Have attach and detach be components
    public override Task OnGameClientAttachedAsync(GameInstallation gameInstallation, GameClientInstallation gameClientInstallation)
    {
        // Create config file
        new AutoConfigManager(GetGameConfigFile(gameInstallation)).Create(gameInstallation);

        return Task.CompletedTask;
    }

    #endregion
}