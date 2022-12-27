using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro.Games.Emulators.DosBox;

public sealed class DosBoxEmulatorDescriptor : EmulatorDescriptor
{
    #region Public Properties

    public override string EmulatorId => "DOSBox";
    public override GamePlatform[] SupportedPlatforms => new[] { GamePlatform.MsDos };
    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.GameType_DosBox));
    public override EmulatorIconAsset Icon => EmulatorIconAsset.DosBox;

    #endregion

    #region Private Methods

    private static MsDosGameDescriptor GetMsdosGameDescriptor(GameInstallation gameInstallation) =>
        gameInstallation.GameDescriptor as MsDosGameDescriptor 
        ?? throw new InvalidOperationException($"The {nameof(DosBoxEmulatorDescriptor)} can only be used with {nameof(MsDosGameDescriptor)}");

    #endregion

    #region Public Methods

    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register<LaunchGameComponent>(new DosBoxLaunchGameComponent(this));
    }

    public override EmulatorGameConfigViewModel GetGameConfigViewModel(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation) =>
        new DosBoxGameConfigViewModel(gameInstallation, this, GetMsdosGameDescriptor(gameInstallation));

    public override EmulatorOptionsViewModel GetEmulatorOptionsViewModel(EmulatorInstallation emulatorInstallation) =>
        new DosBoxEmulatorOptionsViewModel(emulatorInstallation, this);

    public override Task OnEmulatorSelectedAsync(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation)
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