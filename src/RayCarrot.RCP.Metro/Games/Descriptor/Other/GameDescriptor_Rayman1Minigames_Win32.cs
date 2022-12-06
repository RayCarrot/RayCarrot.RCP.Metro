using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RayCarrot.RCP.Metro.Games.Options;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 minigames (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman1Minigames_Win32 : Win32GameDescriptor
{
    #region Public Override Properties

    public override string GameId => "Rayman1Minigames_Win32";
    public override Game Game => Game.Rayman1Minigames;
    public override GameCategory Category => GameCategory.Other;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.Ray1Minigames;

    public override string DisplayName => "Rayman Minigames";
    public override string DefaultFileName => "RayGames.exe";

    public override GameIconAsset Icon => GameIconAsset.Rayman1Minigames;

    #endregion

    #region Public Methods

    // Can only be downloaded
    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new DownloadGameAddAction(this, new Uri[]
        {
            new(AppURLs.Games_Ray1Minigames_Url),
        })
    };

    public override IEnumerable<GameOptionsViewModel> GetOptionsViewModels(GameInstallation gameInstallation) =>
        base.GetOptionsViewModels(gameInstallation).Concat(new GameOptionsViewModel[]
        {
            new Rayman1MinigamesGameOptionsViewModel(gameInstallation),
        });

    public override Task PostGameAddAsync(GameInstallation gameInstallation)
    {
        // Default to run as admin
        gameInstallation.SetValue(GameDataKey.Win32_RunAsAdmin, true);

        // Call base and return
        return base.PostGameAddAsync(gameInstallation);
    }

    #endregion
}