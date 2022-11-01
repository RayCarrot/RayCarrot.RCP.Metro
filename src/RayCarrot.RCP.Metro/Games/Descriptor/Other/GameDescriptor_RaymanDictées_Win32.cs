namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Dictées (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanDictées_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string Id => "RaymanDictées_Win32";
    public override Game Game => Game.RaymanDictées;
    public override GameCategory Category => GameCategory.Other;
    public override Games LegacyGame => Games.RaymanDictées;

    public override string DisplayName => "Rayman Dictées";
    public override string DefaultFileName => "Dictee.exe";

    #endregion
}