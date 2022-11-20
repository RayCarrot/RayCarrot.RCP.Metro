namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Premiers Clics (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanPremiersClics_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string Id => "RaymanPremiersClics_Win32";
    public override Game Game => Game.RaymanPremiersClics;
    public override GameCategory Category => GameCategory.Other;
    public override Games? LegacyGame => Games.RaymanPremiersClics;

    public override string DisplayName => "Rayman Premiers Clics";
    public override string DefaultFileName => "RAYMAN.exe";

    public override GameIconAsset Icon => GameIconAsset.RaymanPremiersClics;

    #endregion
}