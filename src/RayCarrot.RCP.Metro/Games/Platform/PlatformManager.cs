using System.Collections.Generic;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

public abstract class PlatformManager
{
    protected PlatformManager(GameDescriptor gameDescriptor)
    {
        GameDescriptor = gameDescriptor;
    }

    public GameDescriptor GameDescriptor { get; }
    public abstract GamePlatform Platform { get; }

    /// <summary>
    /// Gets the info items for the game
    /// </summary>
    /// <param name="gameInstallation">The game installation to get the info items for</param>
    /// <returns>The info items</returns>
    public virtual IEnumerable<DuoGridItemViewModel> GetGameInfoItems(GameInstallation gameInstallation)
    {
        GameDescriptor.VerifyGameInstallation(gameInstallation);

        return new DuoGridItemViewModel[]
        {
            // TODO-14: Change this to show the platform
            //new DuoGridItemViewModel(
            //    header: new ResourceLocString(nameof(Resources.GameInfo_GameType)),
            //    text: GameTypeDisplayName,
            //    minUserLevel: UserLevel.Advanced),
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.GameInfo_InstallDir)),
                text: gameInstallation.InstallLocation.FullPath),
            //new DuoGridItemViewModel("Install size", GameData.InstallDirectory.GetSize().ToString())
        };
    }
}