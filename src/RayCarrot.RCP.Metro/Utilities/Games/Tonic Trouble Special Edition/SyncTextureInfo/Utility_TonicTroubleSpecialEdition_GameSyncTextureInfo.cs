namespace RayCarrot.RCP.Metro;

/// <summary>
/// The sync texture info utility for Tonic Trouble Special Edition
/// </summary>
public class Utility_TonicTroubleSpecialEdition_GameSyncTextureInfo : Utility_BaseGameSyncTextureInfo
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_TonicTroubleSpecialEdition_GameSyncTextureInfo() : base(new Utility_BaseGameSyncTextureInfo_ViewModel(Games.TonicTroubleSpecialEdition, CPAGameMode.TonicTrouble_SE_PC, new string[]
    {
        "GameData"
    }))
    { }
}