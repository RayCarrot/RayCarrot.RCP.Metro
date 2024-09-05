namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class SetupGameManager_MsDosGameRequiresDisc : SetupGameManager
{
    public SetupGameManager_MsDosGameRequiresDisc(GameInstallation gameInstallation) : base(gameInstallation) { }

    public override IEnumerable<SetupGameAction> GetIssueActions()
    {
        FileSystemPath mountPath = GameInstallation.GetValue<FileSystemPath>(GameDataKey.Client_DosBox_MountPath);
        if (!mountPath.Exists)
        {
            // TODO-LOC
            yield return new SetupGameAction(
                header: "Missing mount path",
                info: "A valid mount path has to be specified in the game config for it to be able to launch.",
                isComplete: false,
                fixActionIcon: GenericIconKind.SetupGame_Config,
                fixActionDisplayName: "Open config",
                fixAction: async () => await Services.UI.ShowGameOptionsAsync(GameInstallation));
        }
    }
}