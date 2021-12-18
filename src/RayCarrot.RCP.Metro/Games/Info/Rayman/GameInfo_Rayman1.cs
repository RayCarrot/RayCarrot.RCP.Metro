#nullable disable
using System.Collections.Generic;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 game info
/// </summary>
public sealed class GameInfo_Rayman1 : GameInfo
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.Rayman1;

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

    /// <summary>
    /// The config page view model, if any is available
    /// </summary>
    public override GameOptionsDialog_ConfigPageViewModel ConfigPageViewModel => new Config_Rayman1_ViewModel(Game);

    /// <summary>
    /// The options UI, if any is available
    /// </summary>
    public override FrameworkElement OptionsUI => new GameOptions_DOSBox_UI(Game);

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels => new ProgressionGameViewModel_Rayman1().Yield();

    /// <summary>
    /// Optional RayMap URL
    /// </summary>
    public override string RayMapURL => AppURLs.GetRay1MapGameURL("RaymanPC_1_21", "r1/pc_121");

    /// <summary>
    /// Gets the file links for the game
    /// </summary>
    public override IList<GameFileLink> GetGameFileLinks => new GameFileLink[0];

    /// <summary>
    /// An optional emulator to use for the game
    /// </summary>
    public override Emulator Emulator => new Emulator_DOSBox(Game, GameType.DosBox);

    #endregion
}