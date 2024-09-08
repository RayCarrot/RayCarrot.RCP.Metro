namespace RayCarrot.RCP.Metro.Games.SetupGame;

public abstract class SetupGameActionsGroupViewModel : BaseViewModel
{
    protected SetupGameActionsGroupViewModel(GameInstallation gameInstallation, IEnumerable<SetupGameAction> actions)
    {
        Actions = new ObservableCollection<SetupGameActionViewModel>(actions.Select(x => new SetupGameActionViewModel(gameInstallation, x)));
    }

    public abstract LocalizedString Header { get; }
   
    public abstract LocalizedString SummaryText { get; }
    public abstract SetupGameActionState SummaryState { get; }
    public abstract bool ShowSummary { get; }

    public ObservableCollection<SetupGameActionViewModel> Actions { get; }
    public int CompletedActions => Actions.Count(x => x.IsComplete);
    public int TotalActions => Actions.Count;
}