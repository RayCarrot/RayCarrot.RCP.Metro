#nullable disable
using System.Collections.Generic;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman1_MSDOS : MSDOSGameDescriptor
{
    #region Public Override Properties

    public override string Id => "Rayman1_MSDOS";
    public override Game Game => Game.Rayman1;

    /// <summary>
    /// The game
    /// </summary>
    public override Games LegacyGame => Games.Rayman1;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Rayman;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => "Rayman 1";

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "Rayman.exe";

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_Rayman1_ViewModel(gameInstallation);

    public override FrameworkElement GetOptionsUI(GameInstallation gameInstallation) => 
        new GameOptions_DOSBox_Control(gameInstallation);

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) => 
        new ProgressionGameViewModel_Rayman1(gameInstallation).Yield();

    /// <summary>
    /// Optional RayMap URL
    /// </summary>
    public override string RayMapURL => AppURLs.GetRay1MapGameURL("RaymanPC_1_21", "r1/pc_121");

    /// <summary>
    /// An optional emulator to use for the game
    /// </summary>
    public override Emulator Emulator => new Emulator_DOSBox();

    #endregion

    #region Public Override Methods

    public override IEnumerable<Utility> GetUtilities(GameInstallation gameInstallation) => new Utility[]
    {
        new Utility_Rayman1_TPLS(gameInstallation),
        new Utility_Rayman1_CompleteSoundtrack(gameInstallation),
    };

    #endregion
}