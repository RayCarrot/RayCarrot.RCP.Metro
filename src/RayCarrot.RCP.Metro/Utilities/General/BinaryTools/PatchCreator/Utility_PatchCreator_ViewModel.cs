using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.RCP.Metro.Patcher;

namespace RayCarrot.RCP.Metro;

public class Utility_PatchCreator_ViewModel : BaseRCPViewModel
{
    #region Constructor

    public Utility_PatchCreator_ViewModel()
    {
        Games = new ObservableCollection<GameItem>(App.GetGames.Select(x => new GameItem(x, x.GetGameInfo().DisplayName)));
        SelectedGame = Games.First();

        CreatePatchCommand = new AsyncRelayCommand(CreateArchivePatchAsync);
        UpdatePatchCommand = new AsyncRelayCommand(UpdateArchivePatchAsync);
    }

    #endregion

    #region Commands

    public ICommand CreatePatchCommand { get; }
    public ICommand UpdatePatchCommand { get; }

    #endregion

    #region Public Properties

    public ObservableCollection<GameItem> Games { get; }
    public GameItem SelectedGame { get; set; }

    #endregion

    #region Public Methods

    public async Task CreateArchivePatchAsync()
    {
        if (!SelectedGame.Game.IsAdded())
        {
            await Services.MessageUI.DisplayMessageAsync("You can only create a patch for a game that you have added to the Rayman Control Panel", MessageType.Error);
            return;
        }

        // Show the Patch Creator
        await Services.UI.ShowPatchCreatorAsync(SelectedGame.Game, null);
    }

    public async Task UpdateArchivePatchAsync()
    {
        if (!SelectedGame.Game.IsAdded())
        {
            await Services.MessageUI.DisplayMessageAsync("You can only create a patch for a game that you have added to the Rayman Control Panel", MessageType.Error);
            return;
        }

        // TODO-UPDATE: Localize
        FileBrowserResult browseResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel
        {
            Title = "Select a patch to import from",
            ExtensionFilter = new FileFilterItem($"*{PatchFile.FileExtension}", "Game Patch").StringRepresentation,
        });

        if (browseResult.CanceledByUser)
            return;

        // Show the Patch Creator
        await Services.UI.ShowPatchCreatorAsync(SelectedGame.Game, browseResult.SelectedFile);
    }

    #endregion

    #region Data Types

    public record GameItem(Games Game, string DisplayName);

    #endregion
}