namespace RayCarrot.RCP.Metro;

/// <summary>
/// The sync texture info utility for Rayman Arena
/// </summary>
public class Utility_RaymanArena_GameSyncTextureInfo : Utility_BaseGameSyncTextureInfo
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_RaymanArena_GameSyncTextureInfo(GameInstallation gameInstallation) 
        : base(new Utility_BaseGameSyncTextureInfo_ViewModel(gameInstallation, CPAGameMode.RaymanArena_PC, new string[]
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