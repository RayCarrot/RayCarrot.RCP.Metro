using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The sync texture info utility for Rayman M
/// </summary>
public class Utility_RaymanM_GameSyncTextureInfo : Utility_BaseGameSyncTextureInfo
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_RaymanM_GameSyncTextureInfo() : base(new Utility_BaseGameSyncTextureInfo_ViewModel(Games.RaymanM, GameMode.RaymanMPC, new string[]
    {
        "MenuBin",
        "TribeBin",
        "FishBin",
    }))
    { }
}