namespace RayCarrot.RCP.Metro;

/// <summary>
/// The sync texture info utility for Rayman 3
/// </summary>
public class Utility_Rayman3_GameSyncTextureInfo : Utility_BaseGameSyncTextureInfo
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_Rayman3_GameSyncTextureInfo() : base(new Utility_BaseGameSyncTextureInfo_ViewModel(Games.Rayman3, OpenSpaceGameMode.Rayman3_PC, new string[]
    {
        "Gamedatabin"
    }))
    { }
}