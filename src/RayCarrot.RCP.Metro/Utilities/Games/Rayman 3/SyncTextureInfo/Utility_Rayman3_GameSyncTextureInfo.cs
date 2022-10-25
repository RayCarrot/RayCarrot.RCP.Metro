namespace RayCarrot.RCP.Metro;

/// <summary>
/// The sync texture info utility for Rayman 3
/// </summary>
public class Utility_Rayman3_GameSyncTextureInfo : Utility_BaseGameSyncTextureInfo
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_Rayman3_GameSyncTextureInfo(GameInstallation gameInstallation) 
        : base(new Utility_BaseGameSyncTextureInfo_ViewModel(gameInstallation, CPAGameMode.Rayman3_PC, new string[]
        {
            "Gamedatabin"
        }))
    {
        GameInstallation = gameInstallation;
    }
    public GameInstallation GameInstallation { get; }

}