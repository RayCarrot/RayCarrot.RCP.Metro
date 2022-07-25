namespace RayCarrot.RCP.Metro;

/// <summary>
/// The sync texture info utility for Tonic Trouble
/// </summary>
public class Utility_TonicTrouble_GameSyncTextureInfo : Utility_BaseGameSyncTextureInfo
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_TonicTrouble_GameSyncTextureInfo() : base(new Utility_BaseGameSyncTextureInfo_ViewModel(Games.TonicTrouble, CPAGameMode.TonicTrouble_PC, new string[]
    {
        "gamedata"
    }))
    { }
}