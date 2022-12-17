using RayCarrot.RCP.Metro.Games.Components;
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

    public override LocalizedString DisplayName => "Rayman Minigames";
    public override string DefaultFileName => "RayGames.exe";
    public override DateTime ReleaseDate => new(1999, 07, 20);

    public override GameIconAsset Icon => GameIconAsset.Rayman1Minigames;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(DescriptorComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new GameOptionsComponent(x => new Rayman1MinigamesGameOptionsViewModel(x)));
    }

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

    public override Task PostGameAddAsync(GameInstallation gameInstallation)
    {
        // Default to run as admin
        gameInstallation.SetValue(GameDataKey.Win32_RunAsAdmin, true);

        // Call base and return
        return base.PostGameAddAsync(gameInstallation);
    }

    #endregion
}