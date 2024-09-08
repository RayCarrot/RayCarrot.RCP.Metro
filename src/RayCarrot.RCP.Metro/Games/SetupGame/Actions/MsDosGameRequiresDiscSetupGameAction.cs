﻿namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class MsDosGameRequiresDiscSetupGameAction : SetupGameAction
{
    // TODO-LOC
    public override LocalizedString Header => "Missing mount path";
    public override LocalizedString Info => "A valid mount path has to be specified in the game config for it to be able to launch.";

    public override SetupGameActionType Type => SetupGameActionType.Issue;

    public override GenericIconKind FixActionIcon => GenericIconKind.SetupGame_Config;
    public override LocalizedString? FixActionDisplayName => "Open config"; // TODO-LOC

    public override bool CheckIsAvailable(GameInstallation gameInstallation)
    {
        FileSystemPath mountPath = gameInstallation.GetValue<FileSystemPath>(GameDataKey.Client_DosBox_MountPath);
        return !mountPath.Exists;
    }

    public override bool CheckIsComplete(GameInstallation gameInstallation)
    {
        return false;
    }

    public override async Task FixAsync(GameInstallation gameInstallation)
    {
        await Services.UI.ShowGameOptionsAsync(gameInstallation);
    }
}