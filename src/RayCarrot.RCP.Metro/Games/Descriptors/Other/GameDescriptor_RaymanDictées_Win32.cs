namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Dictées (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanDictées_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string GameId => "RaymanDictées_Win32";
    public override Game Game => Game.RaymanDictées;
    public override GameCategory Category => GameCategory.Other;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.RaymanDictées;

    public override LocalizedString DisplayName => "Rayman Dictées";
    public override string DefaultFileName => "Dictee.exe";
    public override DateTime ReleaseDate => new(1998, 01, 01); // Not exact

    public override GameIconAsset Icon => GameIconAsset.RaymanDictées;

    #endregion
}