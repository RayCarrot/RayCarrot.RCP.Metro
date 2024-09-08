namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class RequiresLaunchComponentSetupGameAction : SetupGameAction
{
    // TODO-LOC
    public override LocalizedString Header => "No game client/emulator selected";
    public override LocalizedString Info => "This game can't be launched without a game client/emulator. Make sure you first add a supported game client/emulator and then select it for use with this game.";

    public override SetupGameActionType Type => SetupGameActionType.Issue;

    public override GenericIconKind FixActionIcon => GenericIconKind.SetupGame_GameClient;
    public override LocalizedString? FixActionDisplayName => "Add game client/emulator"; // TODO-LOC

    public override bool CheckIsAvailable(GameInstallation gameInstallation)
    {
        return !gameInstallation.HasComponent<LaunchGameComponent>();
    }

    public override bool CheckIsComplete(GameInstallation gameInstallation)
    {
        return false;
    }

    public override async Task FixAsync(GameInstallation gameInstallation)
    {
        await Services.UI.ShowGameClientsSetupAsync();
    }
}