using System.Windows.Input;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class SetupGameViewModel : BaseViewModel
{
    public SetupGameViewModel(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;
        ActionGroupSummaries = new ObservableCollectionEx<SetupGameActionsGroupSummaryViewModel>();
        ActionGroupSummaries.EnableCollectionSynchronization();
        ActionGroups = new ObservableCollectionEx<SetupGameActionsGroupViewModel>();
        ActionGroups.EnableCollectionSynchronization();

        RefreshCommand = new AsyncRelayCommand(LoadAsync);
    }

    public ICommand RefreshCommand { get; }

    public GameInstallation GameInstallation { get; }
    public ObservableCollectionEx<SetupGameActionsGroupSummaryViewModel> ActionGroupSummaries { get; }
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
                ActionGroups.ModifyCollection(actionGroups =>
                {
                    actionGroups.Clear();

                    ActionGroupSummaries.ModifyCollection(actionGroupSummaries =>
                    {
                        actionGroupSummaries.Clear();

                        // Recommended actions
                        List<SetupGameAction> recommendedActions = actions.Where(a => a.Type == SetupGameActionType.Recommended).ToList();
                        SetupGameActionsGroupViewModel? recommendedActionsGroup = null;
                        if (recommendedActions.Any())
                            actionGroups.Add(recommendedActionsGroup = new SetupGameActionsGroupViewModel(GameInstallation, new ResourceLocString(nameof(Resources.SetupGame_RecommendedGroupHeader)), recommendedActions));

                        actionGroupSummaries.Add(new SetupGameActionsGroupSummaryViewModel(
                            text: new ResourceLocString(nameof(Resources.SetupGame_RecommendedGroupSummary), 
                                recommendedActionsGroup?.CompletedActions ?? 0, recommendedActionsGroup?.TotalActions ?? 0),
                            state: recommendedActionsGroup == null || recommendedActionsGroup.CompletedActions == recommendedActionsGroup.TotalActions
                                ? SetupGameActionState.Complete
                                : SetupGameActionState.Incomplete));

                        // Optional actions
                        List<SetupGameAction> optionalActions = actions.Where(a => a.Type == SetupGameActionType.Optional).ToList();
                        SetupGameActionsGroupViewModel? optionalActionsGroup = null;
                        if (optionalActions.Any())
                            actionGroups.Add(optionalActionsGroup = new SetupGameActionsGroupViewModel(GameInstallation, new ResourceLocString(nameof(Resources.SetupGame_OptionalGroupHeader)), optionalActions));

                        actionGroupSummaries.Add(new SetupGameActionsGroupSummaryViewModel(
                            text: new ResourceLocString(nameof(Resources.SetupGame_OptionalGroupSummary),
                                optionalActionsGroup?.CompletedActions ?? 0, optionalActionsGroup?.TotalActions ?? 0),
                            state: optionalActionsGroup == null || optionalActionsGroup.CompletedActions == optionalActionsGroup.TotalActions
                                ? SetupGameActionState.Complete
                                : SetupGameActionState.Incomplete));

                        // Issue actions
                        List<SetupGameAction> issueActions = actions.Where(a => a.Type == SetupGameActionType.Issue).ToList();
                        SetupGameActionsGroupViewModel? issueActionsGroup = null;
                        if (issueActions.Any())
                            actionGroups.Add(issueActionsGroup = new SetupGameActionsGroupViewModel(GameInstallation, new ResourceLocString(nameof(Resources.SetupGame_IssuesGroupHeader)), issueActions));

                        actionGroupSummaries.Add(new SetupGameActionsGroupSummaryViewModel(
                            text: new ResourceLocString(issueActionsGroup?.TotalActions == 1 
                                    ? nameof(Resources.SetupGame_IssuesGroupSummarySingle) 
                                    : nameof(Resources.SetupGame_IssuesGroupSummaryMultiple), 
                                issueActionsGroup?.TotalActions ?? 0),
                            state: issueActionsGroup == null || issueActionsGroup.TotalActions == 0
                                ? SetupGameActionState.Complete
                                : SetupGameActionState.Critical));
                    });
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