namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rabbids Coding (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RabbidsCoding_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string GameId => "RabbidsCoding_Win32";
    public override Game Game => Game.RabbidsCoding;
    public override GameCategory Category => GameCategory.Rabbids;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.RabbidsCoding;

    public override string DisplayName => "Rabbids Coding";
    public override string DefaultFileName => "Rabbids Coding.exe";
    public override DateTime ReleaseDate => new(2019, 10, 08);

    public override GameIconAsset Icon => GameIconAsset.RabbidsCoding;

    #endregion

    #region Public Methods

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_DownloadUplay)), "https://register.ubisoft.com/rabbids-coding/")
    };

    public override GameFinder_GameItem GetGameFinderItem() => new(null, "Rabbids Coding", new[]
    {
        "RabbidsCoding",
        "Rabbids Coding",
    });

    #endregion
}