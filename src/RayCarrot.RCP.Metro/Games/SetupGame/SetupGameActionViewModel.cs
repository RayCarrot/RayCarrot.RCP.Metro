using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class SetupGameActionViewModel : BaseViewModel
{
    public SetupGameActionViewModel(GameInstallation gameInstallation, SetupGameAction action)
    {
        GameInstallation = gameInstallation;
        Action = action;

        bool isComplete = action.CheckIsComplete(gameInstallation);

        State = action.Type switch
        {
            SetupGameActionType.Recommended => isComplete ? SetupGameActionState.Complete : SetupGameActionState.Incomplete,
            SetupGameActionType.Optional => isComplete ? SetupGameActionState.Complete : SetupGameActionState.Incomplete,
            SetupGameActionType.Issue => SetupGameActionState.Critical,
            _ => throw new ArgumentOutOfRangeException()
        };
        Header = action.Header;
        Info = action.Info;
        IsComplete = isComplete;
        FixActionIcon = action.FixActionIcon;
        FixActionDisplayName = action.FixActionDisplayName;

        FixCommand = new AsyncRelayCommand(FixAsync);
    }

    public ICommand? FixCommand { get; }

    public GameInstallation GameInstallation { get; }
    public SetupGameAction Action { get; }

    public SetupGameActionState State { get; }
    public LocalizedString Header { get; }
    public LocalizedString Info { get; }
    public bool IsComplete { get; }

    public GenericIconKind FixActionIcon { get; }
    public LocalizedString? FixActionDisplayName { get; }

    public async Task FixAsync()
    {
        await Action.FixAsync(GameInstallation);
    }
}