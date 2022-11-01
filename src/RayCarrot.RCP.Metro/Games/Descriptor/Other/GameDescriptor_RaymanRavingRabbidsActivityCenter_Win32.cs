using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Raving Rabbids Activity Center (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanRavingRabbidsActivityCenter_Win32 : Win32GameDescriptor
{
    #region Public Override Properties

    public override string Id => "RaymanRavingRabbidsActivityCenter_Win32";
    public override Game Game => Game.RaymanRavingRabbidsActivityCenter;
    public override GameCategory Category => GameCategory.Other;
    public override Games LegacyGame => Games.RaymanRavingRabbidsActivityCenter;

    public override string DisplayName => "Rayman Raving Rabbids Activity Center";
    public override string DefaultFileName => "Rayman.exe";

    public override bool CanBeLocated => false;
    public override bool CanBeDownloaded => true;
    public override IList<Uri> DownloadURLs => new Uri[]
    {
        new(AppURLs.Games_RavingRabbidsActivityCenter_Url)
    };

    #endregion

    #region Protected Methods

    protected override async Task PostLaunchAsync(Process? process)
    {
        // Check if the launch message should show
        if (!Services.Data.Game_ShownRabbidsActivityCenterLaunchMessage)
        {
            await Services.MessageUI.DisplayMessageAsync(Resources.RabbidsActivityCenter_LaunchMessage, MessageType.Information);

            // Flag that the message should not be shown again
            Services.Data.Game_ShownRabbidsActivityCenterLaunchMessage = true;
        }

        // Run the base code
        await base.PostLaunchAsync(process);
    }

    #endregion
}