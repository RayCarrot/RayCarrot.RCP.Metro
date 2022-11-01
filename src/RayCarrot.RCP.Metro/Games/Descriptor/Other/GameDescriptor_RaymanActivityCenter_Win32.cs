namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Activity Center (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanActivityCenter_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string Id => "RaymanActivityCenter_Win32";
    public override Game Game => Game.RaymanActivityCenter;
    public override GameCategory Category => GameCategory.Other;
    public override Games LegacyGame => Games.RaymanActivityCenter;

    public override string DisplayName => "Rayman Activity Center";
    public override string DefaultFileName => "Rayman.exe";

    #endregion
}