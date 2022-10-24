namespace RayCarrot.RCP.Metro;

/// <summary>
/// The sync texture info utility for Rayman 2
/// </summary>
public class Utility_Rayman2_GameSyncTextureInfo : Utility_BaseGameSyncTextureInfo
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_Rayman2_GameSyncTextureInfo() : base(new Utility_BaseGameSyncTextureInfo_ViewModel(Games.Rayman2.GetInstallation(), CPAGameMode.Rayman2_PC, new string[]
    {
        "Data"
    }))
    { }
}