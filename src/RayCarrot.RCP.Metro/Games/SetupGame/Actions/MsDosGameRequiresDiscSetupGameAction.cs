namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class MsDosGameRequiresDiscSetupGameAction : SetupGameAction
{
    public override LocalizedString Header => new ResourceLocString(nameof(Resources.SetupGameAction_MsDosGameRequiresDisc_Header));
    public override LocalizedString Info => new ResourceLocString(nameof(Resources.SetupGameAction_MsDosGameRequiresDisc_Info));

    public override SetupGameActionType Type => SetupGameActionType.Issue;

    public override GenericIconKind FixActionIcon => GenericIconKind.SetupGame_GameClientGameSettings;
    public override LocalizedString FixActionDisplayName => new ResourceLocString(nameof(Resources.SetupGameAction_GameClientGameSettingsFix));

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
        await Services.UI.ShowGameClientGameOptionsAsync(gameInstallation);
    }
}