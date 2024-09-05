namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class SetupGameManager_RequiresLaunchComponent : SetupGameManager
{
    public SetupGameManager_RequiresLaunchComponent(GameInstallation gameInstallation) : base(gameInstallation) { }

    public override IEnumerable<SetupGameAction> GetIssueActions()
    {
        if (!GameInstallation.HasComponent<LaunchGameComponent>())
        {
            // TODO-LOC
            yield return new SetupGameAction(
                header: "No game client/emulator selected",
                info: "This game can't be launched without a game client/emulator. Make sure you first add a supported game client/emulator and then select it for use with this game.",
                isComplete: false,
                fixActionIcon: GenericIconKind.SetupGame_GameClient,
                fixActionDisplayName: "Add game client/emulator",
                fixAction: async () => await Services.UI.ShowGameClientsSetupAsync());
        }
    }
}