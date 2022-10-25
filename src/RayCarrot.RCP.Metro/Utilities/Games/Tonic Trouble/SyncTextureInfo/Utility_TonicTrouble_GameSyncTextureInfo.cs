namespace RayCarrot.RCP.Metro;

/// <summary>
/// The sync texture info utility for Tonic Trouble
/// </summary>
public class Utility_TonicTrouble_GameSyncTextureInfo : Utility_BaseGameSyncTextureInfo
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_TonicTrouble_GameSyncTextureInfo(GameInstallation gameInstallation) 
        : base(new Utility_BaseGameSyncTextureInfo_ViewModel(gameInstallation, CPAGameMode.TonicTrouble_PC, new string[]
        {
            "gamedata"
        }))
    {
        GameInstallation = gameInstallation;
    }

    public GameInstallation GameInstallation { get; }
}