namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class SetupGameActionsGroupViewModel : BaseViewModel
{
    public SetupGameActionsGroupViewModel(GameInstallation gameInstallation, LocalizedString header, IEnumerable<SetupGameAction> actions)
    {
        Header = header;
        Actions = new ObservableCollection<SetupGameActionViewModel>(actions.Select(x => new SetupGameActionViewModel(gameInstallation, x)));
    }

    public LocalizedString Header { get; }
    public ObservableCollection<SetupGameActionViewModel> Actions { get; }
    public int CompletedActions => Actions.Count(x => x.IsComplete);
    public int TotalActions => Actions.Count;
}