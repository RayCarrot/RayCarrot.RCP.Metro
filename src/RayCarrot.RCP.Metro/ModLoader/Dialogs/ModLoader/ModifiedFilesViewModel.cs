using System.Windows.Input;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.ModLoader.Resource;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

public class ModifiedFilesViewModel : BaseViewModel
{
    #region Constructor

    public ModifiedFilesViewModel(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;
        ModifiedFilesRoot = new ModifiedFileItemViewModel("root");
        ShowModifiedFilesAsTree = true;
        _invalidLocations = new Dictionary<string, bool>
        {
            // The game location is always valid
            [String.Empty] = false,
        };

        ExpandAllCommand = new RelayCommand(ExpandAll);
    }

    #endregion

    #region Private Fields

    private static readonly char[] _pathSeparators = { '/', '\\' };
    private readonly Dictionary<string, bool> _invalidLocations;

    #endregion

    #region Commands

    public ICommand ExpandAllCommand { get; }

    #endregion

    #region Properties

    public GameInstallation GameInstallation { get; }
    public ModifiedFileItemViewModel ModifiedFilesRoot { get; }
    public bool ShowModifiedFilesAsTree { get; set; }

    public int AddedFilesCount { get; private set; }
    public int RemovedFilesCount { get; private set; }
    public int PatchedFilesCount { get; private set; }

    public bool HasConflicts { get; private set; }

    #endregion

    #region Private Methods

    private static ModifiedFileItemViewModel AddFolder(ModifiedFileItemViewModel root, bool isArchive, string[] folders, int? count = null)
    {
        count ??= folders.Length;
        ModifiedFileItemViewModel currentItem = root;

        for (int i = 0; i < count; i++)
        {
            ModifiedFileItemViewModel? existingItem = currentItem.GetOverridableItem(folders[i]);

            if (existingItem == null)
            {
                ModifiedFileItemViewModel.ItemType type = i == count - 1 && isArchive
                    ? ModifiedFileItemViewModel.ItemType.Archive
                    : ModifiedFileItemViewModel.ItemType.Folder;
                existingItem = new ModifiedFileItemViewModel(folders[i], null, type, ModifiedFileItemViewModel.FileModification.None, false);
                currentItem.AddItem(existingItem, true);
            }

            currentItem = existingItem;
        }

        return currentItem;
    }

    private void AddFile(ModFilePath filePath, Mod mod, ModifiedFileItemViewModel.FileModification modification, bool isOverridable)
    {
        ModifiedFileItemViewModel currentItem = ModifiedFilesRoot;
        string[] pathParts = filePath.FilePath.Split(_pathSeparators);

        if (ShowModifiedFilesAsTree)
        {
            // Add the folders for the archive
            if (filePath.HasLocation)
                currentItem = AddFolder(currentItem, true, filePath.Location.Split(_pathSeparators));

            // Add the folders
            currentItem = AddFolder(currentItem, false, pathParts, pathParts.Length - 1);
        }

        string fileName = ShowModifiedFilesAsTree ? pathParts.Last() : filePath.ToString();

        // See if one already exists
        if (isOverridable && currentItem.GetOverridableItem(fileName) is { } existingFileItem)
        {
            existingFileItem.OverridenMods.Add(mod.Metadata.Name);
            HasConflicts = true;
        }
        else
        {
            // Make sure the location exists for the file. Right now two cases causes files to be ignored:
            // - No archive component with the location id exists for this game
            // - The archive location file doesn't exist
            // Both of these will have the patcher silently ignore the files while logging a warning
            if (!_invalidLocations.ContainsKey(filePath.Location))
            {
                ArchiveComponent? archiveComponent = GameInstallation.GetComponents<ArchiveComponent>().
                    FirstOrDefault(x => x.Id == filePath.LocationID);
                FileSystemPath archivePath = GameInstallation.InstallLocation.Directory + filePath.Location;

                _invalidLocations.Add(filePath.Location, archiveComponent == null || !archivePath.FileExists);
            }
            currentItem.AddItem(new ModifiedFileItemViewModel(fileName, mod.Metadata.Name,
                ModifiedFileItemViewModel.ItemType.File, modification, _invalidLocations[filePath.Location]), isOverridable);

            if (modification == ModifiedFileItemViewModel.FileModification.Add)
                AddedFilesCount++;
            else if (modification == ModifiedFileItemViewModel.FileModification.Remove)
                RemovedFilesCount++;
            else if (modification == ModifiedFileItemViewModel.FileModification.Patch)
                PatchedFilesCount++;
        }
    }

    #endregion

    #region Public Methods

    public void Refresh(IEnumerable<ModViewModel> enabledMods)
    {
        AddedFilesCount = 0;
        RemovedFilesCount = 0;
        PatchedFilesCount = 0;
        HasConflicts = false;

        foreach (ModViewModel modViewModel in enabledMods)
        {
            Mod mod = modViewModel.Mod;

            foreach (IModFileResource addedFile in mod.GetAddedFiles())
                AddFile(addedFile.Path, mod, ModifiedFileItemViewModel.FileModification.Add, true);

            foreach (ModFilePath removedFile in mod.GetRemovedFiles())
                AddFile(removedFile, mod, ModifiedFileItemViewModel.FileModification.Remove, true);

            foreach (IFilePatch patchedFile in mod.GetPatchedFiles())
                AddFile(patchedFile.Path, mod, ModifiedFileItemViewModel.FileModification.Patch, false);
        }

        ModifiedFilesRoot.ApplyItems();
    }

    public void ExpandAll()
    {
        expandAll(ModifiedFilesRoot);

        static void expandAll(ModifiedFileItemViewModel item)
        {
            item.IsExpanded = true;

            foreach (ModifiedFileItemViewModel child in item.Children)
                expandAll(child);
        }
    }

    #endregion
}