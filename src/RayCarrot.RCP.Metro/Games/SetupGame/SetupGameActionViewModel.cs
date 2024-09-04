using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class SetupGameActionViewModel : BaseViewModel
{
    public SetupGameActionViewModel(SetupGameActionState state, SetupGameAction action)
    {
        State = state;
        Header = action.Header;
        Info = action.Info;
        IsComplete = action.IsComplete;
        FixActionIcon = action.FixActionIcon;
        FixActionDisplayName = action.FixActionDisplayName;

        FixAction = action.FixAction;
        HasFixAction = action.FixAction != null && !IsComplete;
        
        FixCommand = new AsyncRelayCommand(FixAsync);
    }

    public ICommand? FixCommand { get; }

    public SetupGameActionState State { get; }
    public LocalizedString Header { get; }
    public LocalizedString Info { get; }
    public bool IsComplete { get; }

    public Func<Task>? FixAction { get; }
    public bool HasFixAction { get; }

    public GenericIconKind FixActionIcon { get; }
    public LocalizedString? FixActionDisplayName { get; }

    public async Task FixAsync()
    {
        if (FixAction == null)
            return;

        await FixAction();
    }
}