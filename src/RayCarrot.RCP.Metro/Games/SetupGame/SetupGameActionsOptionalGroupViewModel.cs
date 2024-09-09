namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class SetupGameActionsOptionalGroupViewModel : SetupGameActionsGroupViewModel
{
    public SetupGameActionsOptionalGroupViewModel(GameInstallation gameInstallation, IEnumerable<SetupGameAction> actions)
        : base(gameInstallation, actions)
    {
        // TODO-LOC
        Header = "Optional";

        SummaryText = $"{CompletedActions}/{TotalActions} optional actions";
        SummaryState = CompletedActions == TotalActions
            ? SetupGameActionState.Complete
            : SetupGameActionState.Incomplete;
    }

    public override LocalizedString Header { get; }

    public override LocalizedString SummaryText { get; }
    public override SetupGameActionState SummaryState { get; }
}