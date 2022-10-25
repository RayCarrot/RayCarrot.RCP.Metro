namespace RayCarrot.RCP.Metro;

/// <summary>
/// The sync texture info utility for Tonic Trouble Special Edition
/// </summary>
public class Utility_TonicTroubleSpecialEdition_GameSyncTextureInfo : Utility_BaseGameSyncTextureInfo
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_TonicTroubleSpecialEdition_GameSyncTextureInfo(GameInstallation gameInstallation) 
        : base(new Utility_BaseGameSyncTextureInfo_ViewModel(gameInstallation, CPAGameMode.TonicTrouble_SE_PC, new string[]
        {
            "GameData"
        }))
    {
        GameInstallation = gameInstallation;
    }

    public GameInstallation GameInstallation { get; }
}