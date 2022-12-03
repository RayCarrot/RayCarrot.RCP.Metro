using System.Linq;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extension methods for <see cref="LegacyGame"/>
/// </summary>
public static class GamesExtensions
{
    // TODO-14: Remove once no longer needed
    public static GameInstallation GetInstallation(this LegacyGame game) => 
        Services.Data.Game_GameInstallations.First(x => x.LegacyGame == game);
}