namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class BetterRayman3SetupGameAction : SetupGameAction
{
    // TODO-LOC
    public override LocalizedString Header => "Install Better Rayman 3";
    public override LocalizedString Info => "Better Rayman 3 is a collection of fixes by RibShark that allow Rayman 3 to be easily played in widescreen resolutions, while also offering additional features, such as windowed mode and restoring the ability to skip video cutscenes.";

    public override SetupGameActionType Type => SetupGameActionType.Recommended;

    public override GenericIconKind FixActionIcon => GenericIconKind.SetupGame_Download;
    public override LocalizedString? FixActionDisplayName => "Download"; // TODO-LOC

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