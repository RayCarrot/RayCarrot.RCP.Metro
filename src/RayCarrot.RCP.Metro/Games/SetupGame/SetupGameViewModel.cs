using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class SetupGameViewModel : BaseViewModel
{
    public SetupGameViewModel(SetupGameManager setupGameManager)
    {
        SetupGameManager = setupGameManager;
        ActionGroups = new ObservableCollectionEx<SetupGameActionsGroupViewModel>();
        ActionGroups.EnableCollectionSynchronization();

        RefreshCommand = new AsyncRelayCommand(LoadAsync);
    }

    public ICommand RefreshCommand { get; }

    public SetupGameManager SetupGameManager { get; }
    public ObservableCollectionEx<SetupGameActionsGroupViewModel> ActionGroups { get; }
    public bool IsLoading { get; set; }

    public async Task LoadAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;

        try
        {
            await Task.Run(() =>
            {
                ActionGroups.ModifyCollection(x =>
                {
                    x.Clear();

                    x.Add(new SetupGameActionsRecommendedGroupViewModel(SetupGameManager.GetRecommendedActions()));
                    x.Add(new SetupGameActionsOptionalGroupViewModel(SetupGameManager.GetOptionalActions()));
                    x.Add(new SetupGameActionsIssueGroupViewModel(SetupGameManager.GetIssueActions()));
                });
            });
        }
        finally
        {
            IsLoading = false;
        }
    }
}