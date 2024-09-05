namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class SetupGameActionsRecommendedGroupViewModel : SetupGameActionsGroupViewModel
{
    public SetupGameActionsRecommendedGroupViewModel(IEnumerable<SetupGameAction> actions) :
        base(new ObservableCollection<SetupGameActionViewModel>(actions.
            Select(action => new SetupGameActionViewModel(
                state: action.IsComplete ? SetupGameActionState.Complete : SetupGameActionState.Incomplete, 
                action: action))))
    {
        // TODO-LOC
        Header = "Recommended";

        SummaryText = $"{CompletedActions}/{TotalActions} recommended actions";
        SummaryState = CompletedActions == TotalActions
            ? SetupGameActionState.Complete
            : SetupGameActionState.Incomplete;
        ShowSummary = TotalActions > 0;
    }

    public override LocalizedString Header { get; }

    public override LocalizedString SummaryText { get; }
    public override SetupGameActionState SummaryState { get; }
    public override bool ShowSummary { get; }
}