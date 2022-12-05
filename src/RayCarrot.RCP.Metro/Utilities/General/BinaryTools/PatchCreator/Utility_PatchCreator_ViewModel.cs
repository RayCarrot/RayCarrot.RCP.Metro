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
        Games = new ObservableCollection<GameItem>(Services.Games.EnumerateGameDescriptors().
            Where(x => x.AllowPatching).
            Select(x => new GameItem(x, x.GameDescriptorName)));
        SelectedGame = Games.First();

        CreatePatchCommand = new AsyncRelayCommand(CreatePatchAsync);
        UpdatePatchCommand = new AsyncRelayCommand(UpdatePatchAsync);
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

    public async Task CreatePatchAsync()
    {
        // Show the Patch Creator
        GameDescriptor[] gameDescriptors = { SelectedGame.GameDescriptor }; // TODO-14: Allow multi-selection
        await Services.UI.ShowPatchCreatorAsync(gameDescriptors, null);
    }

    public async Task UpdatePatchAsync()
    {
        FileBrowserResult browseResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel
        {
            Title = Resources.PatchCreator_SelectImportPatch,
            ExtensionFilter = new FileFilterItem($"*{PatchFile.FileExtension}", Resources.Patcher_FileType).StringRepresentation,
        });

        if (browseResult.CanceledByUser)
            return;

        // Show the Patch Creator
        GameDescriptor[] gameDescriptors = { SelectedGame.GameDescriptor }; // TODO-14: Allow multi-selection
        await Services.UI.ShowPatchCreatorAsync(gameDescriptors, browseResult.SelectedFile);
    }

    #endregion

    #region Data Types

    public record GameItem(GameDescriptor GameDescriptor, LocalizedString DisplayName);

    #endregion
}