using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro.ModLoader.Modules.Files;

public class FilesModuleViewModel : ModModuleViewModel
{
    public FilesModuleViewModel(ModModule module, GameInstallation gameInstallation) : base(module)
    {
        FilesModModuleExamplePaths? examplePathsComponent = gameInstallation.GetComponent<FilesModModuleExamplePaths>();

        Archives = new ObservableCollection<ArchiveViewModel>();
        PathsText = String.Empty;

        // TODO-LOC
        if (examplePathsComponent?.GetExamplePathFunc(String.Empty) is { } examplePath)
            PathsTextHint = $"Example: {examplePath}";

        foreach (ArchiveComponent archiveComponent in gameInstallation.GetComponents<ArchiveComponent>())
        {
            foreach (string archiveFilePath in archiveComponent.GetArchiveFilePaths())
            {
                // TODO-LOC
                LocalizedString? archivePathsTextHint = null;
                if (examplePathsComponent?.GetExamplePathFunc(archiveFilePath) is { } archiveExamplePath)
                    archivePathsTextHint = $"Example: {archiveExamplePath}";

                Archives.Add(new ArchiveViewModel(archiveFilePath, archivePathsTextHint));
            }
        }
    }

    public ObservableCollection<ArchiveViewModel> Archives { get; }
    public LocalizedString? PathsTextHint { get; }
    public string PathsText { get; set; }

    public class ArchiveViewModel : BaseViewModel
    {
        public ArchiveViewModel(string filePath, LocalizedString? pathsTextHint)
        {
            FilePath = filePath;
            DisplayName = filePath;
            PathsTextHint = pathsTextHint;
            PathsText = String.Empty;
        }

        public string FilePath { get; }
        public string DisplayName { get; }
        
        public bool IsEnabled { get; set; }
        public LocalizedString? PathsTextHint { get; }
        public string PathsText { get; set; }
    }
}