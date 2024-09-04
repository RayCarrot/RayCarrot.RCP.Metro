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
                // Get managers
                List<SetupGameManager> managers = GameInstallation.GetComponents<SetupGameManagerComponent>().CreateObjects().ToList();

                // Reload collections
                ActionGroups.ModifyCollection(x =>
                {
                    x.Clear();

                    x.Add(new SetupGameActionsRecommendedGroupViewModel(managers.SelectMany(s => s.GetRecommendedActions())));
                    x.Add(new SetupGameActionsOptionalGroupViewModel(managers.SelectMany(s => s.GetOptionalActions())));
                    x.Add(new SetupGameActionsIssueGroupViewModel(managers.SelectMany(s => s.GetIssueActions())));
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