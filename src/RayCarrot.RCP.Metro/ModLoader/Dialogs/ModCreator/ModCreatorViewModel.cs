using System.IO;
using System.Text;
using System.Windows.Input;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Legacy.Patcher;
using RayCarrot.RCP.Metro.ModLoader.Extractors;
using RayCarrot.RCP.Metro.ModLoader.Modules;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModCreator;

public class ModCreatorViewModel : BaseViewModel
{
    #region Constructor

    public ModCreatorViewModel(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;
        LoaderViewModel = new LoaderViewModel();
        Modules = new ObservableCollection<ModModuleViewModel>(gameInstallation.
            GetComponents<ModModuleComponent>().
            CreateObjects().
            Select(x => x.GetViewModel()));

        // Verify the game supports mods
        if (!Modules.Any())
            throw new InvalidOperationException("The game installation doesn't support mods");

        ConvertLegacyPatchCommand = new AsyncRelayCommand(ConvertLegacyPatchAsync);
        OpenDocumentationCommand = new RelayCommand(OpenDocumentation);
        CreateModCommand = new AsyncRelayCommand(CreateModAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion
    
    #region Commands

    public ICommand ConvertLegacyPatchCommand { get; }
    public ICommand OpenDocumentationCommand { get; }
    public ICommand CreateModCommand { get; }

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }
    public LoaderViewModel LoaderViewModel { get; }
    public ObservableCollection<ModModuleViewModel> Modules { get; }

    #endregion

    #region Private Methods

    private string GetMetadataString()
    {
        string appVersion = AppViewModel.AppVersion.ToString(4);
        string id = Guid.NewGuid().ToString();
        string gameId = GameInstallation.GameDescriptor.GameId;
        const int formatVersion = Mod.LatestFormatVersion;
        string date = DateTime.Now.ToString("yyyy-MM-dd");
        StringBuilder archives = new();

        foreach (ArchiveComponent archiveComponent in GameInstallation.GetComponents<ArchiveComponent>())
        {
            foreach (string archiveFilePath in archiveComponent.GetArchiveFilePaths())
            {
                if (archives.Length > 0)
                    archives.Append(",");

                archives.AppendLine();
                archives.Append($$"""
                                        {
                                            "id": "{{archiveComponent.Id}}",
                                            "path": "{{archiveFilePath.Replace("\\", "\\\\")}}"
                                        }
                                """);
            }
        }

        return $$"""
               //                                        ** Generated with Rayman Control Panel {{appVersion}} **
               {
                   // This is the unique identifier for this mod. It can be any text, but make sure it's not used by any other mod or else both can't
                   // be installed at the same time! The easiest way to avoid collisions is to use the already generated GUID as the id. If you however
                   // want to use a custom id then it is recommended naming it like this: "Game.Type.Name". For example: "RaymanLegends.Costume.Raydoom".
                   "id": "{{id}}",
               
                   // These are the games within the Rayman Control Panel which this mod can be installed to. Each game is identified using the internal
                   // game id in the app. See the mod documentation for an up to date list on all available ids.
                   "games": [
                       "{{gameId}}"
                   ],
               
                   // The format determines for which version of the Rayman Control Panel that the mod was made for, and thus which features it supports.
                   // New features may require a higher format version, but then that mod won't work on older versions of the Rayman Control Panel.
                   "format": {{formatVersion}},
               
                   // This is the mod name. Keep it short, but also clear what the mod does.
                   "name": "",
               
                   // Describe what your mod does in more detail here. Also include optional instructions here. You can specify a new line by typing \n.
                   "description": "",
               
                   // Write your name here. If there are multiple authors then separate them by a comma. For example: "User1, User2"
                   "author": "",
               
                   // Optionally include a link to a website for the mod, such as a GitHub repo, a GameBanana page or forum thread.
                   "website": "",
               
                   // The current version. It must be in the format of "Major.Minor.Revision" where each value is a number. This is used for update checks,
                   // so make sure to update this whenever you update the mod!
                   "version": "1.0.0",
               
                   // Optionally keep a changelog here for the different versions of your mod. The versions are in descending order, so start with the latest.
                   "changelog": [
                       {
                           "version": "1.0.0",
                           "date": "{{date}}",
                           "description": "- Initial release"
                       }
                   ],
               
                   // If you want your mod to modify files within archives then those have to be specified here. The id is the internal archive type id Rayman
                   // Control Panel uses. See the mod documentation for an up to date list on all available ids.
                   "archives": [{{archives}}
                   ]
               }
               """;
    }

    #endregion

    #region Public Methods

    public async Task ConvertLegacyPatchAsync()
    {
        FileBrowserResult browseResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel
        {
            // TODO-LOC
            Title = "Select patch files to convert",
            ExtensionFilter = new FileExtension(PatchPackage.FileExtension).GetFileFilterItem.StringRepresentation,
            MultiSelection = true,
        });

        if (browseResult.CanceledByUser)
            return;

        Logger.Info("Converting legacy patches to mods");

        // TODO-LOC
        using (LoadState state = await LoaderViewModel.RunAsync("Converting patches", true))
        {
            try
            {
                LegacyGamePatchModExtractor extractor = new();
                Progress progress = new(0, browseResult.SelectedFiles.Length);

                foreach (FileSystemPath patchFile in browseResult.SelectedFiles)
                {
                    FileSystemPath outputDir = patchFile.RemoveFileExtension().GetNonExistingDirectoryName();
                    Directory.CreateDirectory(outputDir);

                    await extractor.ExtractAsync(patchFile, outputDir, x => state.SetProgress(progress.Add(x, 1)),
                        state.CancellationToken);

                    progress++;
                }

                WindowsHelpers.OpenExplorerPath(browseResult.SelectedFiles.First().Parent);
            }
            catch (OperationCanceledException ex)
            {
                Logger.Trace(ex, "Cancelled converting legacy patches");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Converting legacy patches");

                // TODO-LOC
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when converting the patches");
            }
        }
        
        Logger.Info("Converted legacy patches to mods");
    }

    public void OpenDocumentation()
    {
        Services.App.OpenUrl("https://github.com/RayCarrot/RayCarrot.RCP.Metro/wiki/Mod-Loader");
    }

    public async Task CreateModAsync()
    {
        Logger.Info("Creating new mod");

        if (!Modules.Any(x => x.IsEnabled))
        {
            Logger.Info("No modules have been enabled");

            // TODO-LOC
            await Services.MessageUI.DisplayMessageAsync("At least one module has to be selected in order to create a mod", MessageType.Error);
            return;
        }

        Logger.Info("The following modules are enabled: {0}", Modules.JoinItems(", ", x => x.Module.Id));

        DirectoryBrowserResult browseResult = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel
        {
            // TODO-LOC
            Title = "Select mod destination"
        });

        if (browseResult.CanceledByUser) 
            return;

        // Write metadata
        string metadataString = GetMetadataString();
        File.WriteAllText(browseResult.SelectedDirectory + Mod.MetadataFileName, metadataString);

        foreach (ModModuleViewModel moduleViewModel in Modules.Where(x => x.IsEnabled))
        {
            FileSystemPath modulePath = browseResult.SelectedDirectory + moduleViewModel.Module.Id;
            Directory.CreateDirectory(modulePath);

            moduleViewModel.Module.SetupModuleFolder(moduleViewModel, modulePath);
        }

        WindowsHelpers.OpenExplorerPath(browseResult.SelectedDirectory);
        
        Logger.Info("Created new mod");
    }

    #endregion
}