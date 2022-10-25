namespace RayCarrot.RCP.Metro;

/// <summary>
/// The sync texture info utility for Rayman M
/// </summary>
public class Utility_RaymanM_GameSyncTextureInfo : Utility_BaseGameSyncTextureInfo
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_RaymanM_GameSyncTextureInfo(GameInstallation gameInstallation) 
        : base(new Utility_BaseGameSyncTextureInfo_ViewModel(gameInstallation, CPAGameMode.RaymanM_PC, new string[]
        {
            "MenuBin",
            "TribeBin",
            "FishBin",
        }))
    {
        GameInstallation = gameInstallation;
    }

    public GameInstallation GameInstallation { get; }
}