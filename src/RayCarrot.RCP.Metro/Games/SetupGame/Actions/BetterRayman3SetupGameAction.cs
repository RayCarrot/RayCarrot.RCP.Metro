namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class BetterRayman3SetupGameAction : SetupGameAction
{
    public override LocalizedString Header => new ResourceLocString(nameof(Resources.SetupGameAction_BetterRayman3_Header));
    public override LocalizedString Info => new ResourceLocString(nameof(Resources.SetupGameAction_BetterRayman3_Info));

    public override SetupGameActionType Type => SetupGameActionType.Recommended;

    public override GenericIconKind FixActionIcon => GenericIconKind.SetupGame_Download;
    public override LocalizedString FixActionDisplayName => new ResourceLocString(nameof(Resources.SetupGameAction_DownloadFix));

    public override bool CheckIsComplete(GameInstallation gameInstallation)
    {
        // Check if Better Rayman 3 is installed
        if ((gameInstallation.InstallLocation.Directory + "BetterRayman3.dll").FileExists)
            return true;

        // Check if Ray3Fix is installed
        if ((gameInstallation.InstallLocation.Directory + @"scripts\BetterRayman3.asi").FileExists)
            return true;

        return false;
    }

    public override Task FixAsync(GameInstallation gameInstallation)
    {
        Services.App.OpenUrl("https://raymanpc.com/forum/viewtopic.php?t=12854"); // Better Rayman 3 forum post
        return Task.CompletedTask;
    }
}