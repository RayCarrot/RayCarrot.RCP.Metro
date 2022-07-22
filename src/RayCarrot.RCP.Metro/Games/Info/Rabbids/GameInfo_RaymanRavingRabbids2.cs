#nullable disable
using System.Collections.Generic;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Raving Rabbids 2 game info
/// </summary>
public sealed class GameInfo_RaymanRavingRabbids2 : GameInfo
{
    #region Public Overrides

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.RaymanRavingRabbids2;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Rabbids;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman Raving Rabbids 2";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => "Rayman Raving Rabbids 2";

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "Jade.exe";

    /// <summary>
    /// The options UI, if any is available
    /// </summary>
    public override FrameworkElement OptionsUI => new GameOptions_RavingRabbids2_Control();

    /// <summary>
    /// The config page view model, if any is available
    /// </summary>
    public override GameOptionsDialog_ConfigPageViewModel ConfigPageViewModel => new Config_RaymanRavingRabbids2_ViewModel(Game);

    /// <summary>
    /// Gets the file links for the game
    /// </summary>
    public override IList<GameFileLink> GetGameFileLinks => new GameFileLink[]
    {
        new GameFileLink(Resources.GameLink_Setup, Game.GetInstallDir() + "SettingsApplication.exe", Arguments: $"/{Services.Data.Game_RRR2LaunchMode.ToString().ToLower()}")
    };

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels => new ProgressionGameViewModel_RaymanRavingRabbids2().Yield();

    #endregion
}