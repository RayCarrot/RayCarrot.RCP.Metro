using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Raving Rabbids Activity Center (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanRavingRabbidsActivityCenter_Win32 : Win32GameDescriptor
{
    #region Public Override Properties

    public override string GameId => "RaymanRavingRabbidsActivityCenter_Win32";
    public override Game Game => Game.RaymanRavingRabbidsActivityCenter;
    public override GameCategory Category => GameCategory.Other;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.RaymanRavingRabbidsActivityCenter;

    public override LocalizedString DisplayName => "Rayman Raving Rabbids Activity Center";
    public override string DefaultFileName => "Rayman.exe";
    public override DateTime ReleaseDate => new(2006, 10, 19);

    public override GameIconAsset Icon => GameIconAsset.RaymanRavingRabbidsActivityCenter;

    #endregion

    #region Private Methods

    private static async Task ShowLaunchMessageAsync(GameInstallation gameInstallation)
    {
        // Check if the launch message should show
        if (!gameInstallation.GetValue<bool>(GameDataKey.RRRAC_ShownLaunchMessage))
        {
            await Services.MessageUI.DisplayMessageAsync(Resources.RabbidsActivityCenter_LaunchMessage, MessageType.Information);

            // Flag that the message should not be shown again
            gameInstallation.SetValue(GameDataKey.RRRAC_ShownLaunchMessage, true);
        }
    }

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new OnGameLaunchedComponent(ShowLaunchMessageAsync));
    }

    // Can only be downloaded
    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new DownloadGameAddAction(this, new Uri[]
        {
            new(AppURLs.Games_RavingRabbidsActivityCenter_Url),
        })
    };

    #endregion
}