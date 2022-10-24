namespace RayCarrot.RCP.Metro;

/// <summary>
/// The sync texture info utility for Rayman Arena
/// </summary>
public class Utility_RaymanArena_GameSyncTextureInfo : Utility_BaseGameSyncTextureInfo
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_RaymanArena_GameSyncTextureInfo() : base(new Utility_BaseGameSyncTextureInfo_ViewModel(Games.RaymanArena.GetInstallation(), CPAGameMode.RaymanArena_PC, new string[]
    {
        "MenuBin",
        "TribeBin",
        "FishBin",
    }))
    { }
}