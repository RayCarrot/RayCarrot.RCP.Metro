namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class SetupGameActionsIssueGroupViewModel : SetupGameActionsGroupViewModel
{
    public SetupGameActionsIssueGroupViewModel(IEnumerable<SetupGameAction> actions) :
        base(new ObservableCollection<SetupGameActionViewModel>(actions.
            Where(x => !x.IsComplete).
            Select(action => new SetupGameActionViewModel(
                state: SetupGameActionState.Critical, 
                action: action))))
    {
        // TODO-LOC
        Header = "Issues";
        EmptyText = "No issues were found";

        SummaryText = $"{TotalActions} issues";
        SummaryState = SetupGameActionState.Critical;
        ShowSummary = TotalActions > 0;
    }

    public override LocalizedString Header { get; }
    public override LocalizedString EmptyText { get; }

    public override LocalizedString SummaryText { get; }
    public override SetupGameActionState SummaryState { get; }
    public override bool ShowSummary { get; }
}