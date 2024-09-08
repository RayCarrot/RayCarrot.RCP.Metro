using System.Windows.Input;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class SetupGameViewModel : BaseViewModel
{
    public SetupGameViewModel(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;
        ActionGroups = new ObservableCollectionEx<SetupGameActionsGroupViewModel>();
        ActionGroups.EnableCollectionSynchronization();

        RefreshCommand = new AsyncRelayCommand(LoadAsync);
    }

    public ICommand RefreshCommand { get; }

    public GameInstallation GameInstallation { get; }
    public ObservableCollectionEx<SetupGameActionsGroupViewModel> ActionGroups { get; }
    public bool IsLoading { get; set; }
    public bool HasActions { get; set; }

    public async Task LoadAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;

        try
        {
            await Task.Run(() =>
            {
                // Get actions
                List<SetupGameAction> actions = GameInstallation.
                    GetComponents<SetupGameActionComponent>().
                    CreateObjects().
                    Where(x => x.CheckIsAvailable(GameInstallation)).
                    ToList();

                // Reload collections
                ActionGroups.ModifyCollection(x =>
                {
                    x.Clear();

                    List<SetupGameAction> recommendedActions = actions.Where(a => a.Type == SetupGameActionType.Recommended).ToList();
                    if (recommendedActions.Any())
                        x.Add(new SetupGameActionsRecommendedGroupViewModel(GameInstallation, recommendedActions));

                    List<SetupGameAction> optionalActions = actions.Where(a => a.Type == SetupGameActionType.Optional).ToList();
                    if (optionalActions.Any())
                        x.Add(new SetupGameActionsOptionalGroupViewModel(GameInstallation, optionalActions));

                    List<SetupGameAction> issueActions = actions.Where(a => a.Type == SetupGameActionType.Issue).ToList();
                    if (issueActions.Any())
                        x.Add(new SetupGameActionsIssueGroupViewModel(GameInstallation, issueActions));
                });

                HasActions = ActionGroups.Any(x => x.Actions.Any());
            });
        }
        finally
        {
            IsLoading = false;
        }
    }
}