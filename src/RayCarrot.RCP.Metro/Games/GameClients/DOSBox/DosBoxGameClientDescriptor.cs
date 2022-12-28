using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro.Games.Clients.DosBox;

public sealed class DosBoxGameClientDescriptor : GameClientDescriptor
{
    #region Public Properties

    public override string GameClientId => "DOSBox";
    public override GamePlatform[] SupportedPlatforms => new[] { GamePlatform.MsDos };
    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.GameType_DosBox));
    public override GameClientIconAsset Icon => GameClientIconAsset.DosBox;

    #endregion

    #region Private Methods

    private static MsDosGameDescriptor GetMsdosGameDescriptor(GameInstallation gameInstallation) =>
        gameInstallation.GameDescriptor as MsDosGameDescriptor 
        ?? throw new InvalidOperationException($"The {nameof(DosBoxGameClientDescriptor)} can only be used with {nameof(MsDosGameDescriptor)}");

    #endregion

    #region Public Methods

    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register<LaunchGameComponent>(new DosBoxLaunchGameComponent(this));
    }

    public override GameClientGameConfigViewModel GetGameConfigViewModel(GameInstallation gameInstallation, GameClientInstallation gameClientInstallation) =>
        new DosBoxGameConfigViewModel(gameInstallation, this, GetMsdosGameDescriptor(gameInstallation));

    public override GameClientOptionsViewModel GetGameClientOptionsViewModel(GameClientInstallation gameClientInstallation) =>
        new DosBoxGameClientOptionsViewModel(gameClientInstallation, this);

    public override Task OnGameClientSelectedAsync(GameInstallation gameInstallation, GameClientInstallation gameClientInstallation)
    {
        // Create config file
        new AutoConfigManager(GetGameConfigFile(gameInstallation)).Create(gameInstallation);

        return Task.CompletedTask;
    }

    // TODO-14: Should be per installation
    /// <summary>
    /// Gets the DosBox configuration file path for the auto config file
    /// </summary>
    /// <returns>The file path</returns>
    public FileSystemPath GetGameConfigFile(GameInstallation gameInstallation) =>
        AppFilePaths.UserDataBaseDir + "DosBox" + (gameInstallation.LegacyGame + ".ini");

    #endregion
}