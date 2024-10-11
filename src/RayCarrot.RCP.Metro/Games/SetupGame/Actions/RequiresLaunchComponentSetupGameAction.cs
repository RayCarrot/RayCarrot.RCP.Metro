namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class RequiresLaunchComponentSetupGameAction : SetupGameAction
{
    public override LocalizedString Header => new ResourceLocString(nameof(Resources.SetupGameAction_RequiresLaunchComponent_Header));
    public override LocalizedString Info => new ResourceLocString(nameof(Resources.SetupGameAction_RequiresLaunchComponent_Info));

    public override SetupGameActionType Type => SetupGameActionType.Issue;

    public override GenericIconKind FixActionIcon => GenericIconKind.SetupGame_GameClient;
    public override LocalizedString? FixActionDisplayName => new ResourceLocString(nameof(Resources.SetupGameAction_RequiresLaunchComponent_Fix));

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