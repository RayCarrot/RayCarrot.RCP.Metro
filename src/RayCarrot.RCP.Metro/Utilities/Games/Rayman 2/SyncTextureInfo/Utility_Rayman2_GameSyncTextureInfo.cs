namespace RayCarrot.RCP.Metro;

/// <summary>
/// The sync texture info utility for Rayman 2
/// </summary>
public class Utility_Rayman2_GameSyncTextureInfo : Utility_BaseGameSyncTextureInfo
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_Rayman2_GameSyncTextureInfo(GameInstallation gameInstallation) 
        : base(new Utility_BaseGameSyncTextureInfo_ViewModel(gameInstallation, CPAGameMode.Rayman2_PC, new string[]
        {
            "Data"
        }))
    {
        GameInstallation = gameInstallation;
    }

    public GameInstallation GameInstallation { get; }
}