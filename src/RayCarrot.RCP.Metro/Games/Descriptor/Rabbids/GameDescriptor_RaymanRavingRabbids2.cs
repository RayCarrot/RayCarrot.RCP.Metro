#nullable disable
using System.Collections.Generic;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Raving Rabbids 2 game info
/// </summary>
public sealed class GameDescriptor_RaymanRavingRabbids2 : GameDescriptor
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

    public override FrameworkElement GetOptionsUI(GameInstallation gameInstallation) => new GameOptions_RavingRabbids2_Control();

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_RaymanRavingRabbids2_ViewModel(gameInstallation);

    /// <summary>
    /// Gets the file links for the game
    /// </summary>
    public override IEnumerable<GameFileLink> GetGameFileLinks(GameInstallation gameInstallation) => new GameFileLink[]
    {
        new(Resources.GameLink_Setup, gameInstallation.InstallLocation + "SettingsApplication.exe", 
            Arguments: $"/{Services.Data.Game_RRR2LaunchMode.ToString().ToLower()}")
    };

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) => 
        new ProgressionGameViewModel_RaymanRavingRabbids2(gameInstallation).Yield();

    #endregion
}