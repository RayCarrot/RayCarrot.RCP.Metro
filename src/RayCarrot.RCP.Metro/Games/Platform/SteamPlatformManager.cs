using System.Collections.Generic;
using System.Linq;

namespace RayCarrot.RCP.Metro;

public class SteamPlatformManager : PlatformManager
{
    public SteamPlatformManager(SteamGameDescriptor gameDescriptor) : base(gameDescriptor)
    {
        GameDescriptor = gameDescriptor;
    }

    public override GamePlatform Platform => GamePlatform.Steam;
    public new SteamGameDescriptor GameDescriptor { get; }

    public override IEnumerable<DuoGridItemViewModel> GetGameInfoItems(GameInstallation gameInstallation) =>
        base.GetGameInfoItems(gameInstallation).Concat(new[]
        {
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.GameInfo_SteamID)),
                text: GameDescriptor.SteamID,
                minUserLevel: UserLevel.Advanced)
        });
}