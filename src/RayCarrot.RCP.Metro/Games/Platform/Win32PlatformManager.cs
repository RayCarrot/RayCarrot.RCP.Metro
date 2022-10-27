using static RayCarrot.RCP.Metro.GameManager_Win32;
using System.Collections.Generic;
using System.Linq;

namespace RayCarrot.RCP.Metro;

public class Win32PlatformManager : PlatformManager
{
    public Win32PlatformManager(Win32GameDescriptor gameDescriptor) : base(gameDescriptor)
    {
        GameDescriptor = gameDescriptor;
    }

    public override GamePlatform Platform => GamePlatform.MSDOS;
    public new Win32GameDescriptor GameDescriptor { get; }

    public override IEnumerable<DuoGridItemViewModel> GetGameInfoItems(GameInstallation gameInstallation)
    {
        // Get the launch info
        GameLaunchInfo launchInfo = ((GameManager_Win32)gameInstallation.GameManager).GetLaunchInfo(gameInstallation);

        return base.GetGameInfoItems(gameInstallation).Concat(new[]
        {
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.GameInfo_LaunchPath)),
                text: launchInfo.Path.FullPath,
                minUserLevel: UserLevel.Technical),
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.GameInfo_LaunchArgs)),
                text: launchInfo.Args,
                minUserLevel: UserLevel.Technical)
        });
    }
}