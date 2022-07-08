using System;
using System.Threading.Tasks;
using NLog;

namespace RayCarrot.RCP.Metro.Patcher;

public class PatcherViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    public PatcherViewModel(Games game)
    {
        LoadOperation = new BindableOperation();

        Game = game;
        Container = new PatchContainerViewModel(game, game.GetInstallDir(), LoadOperation);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public Games Game { get; }
    public PatchContainerViewModel Container { get; }
    public BindableOperation LoadOperation { get; }

    #endregion

    #region Public Methods

    public void DeselectAll()
    {
        Container.SelectedPatch = null;
        Logger.Trace("Deselected all patches");
    }

    public async Task LoadPatchesAsync()
    {
        Logger.Info("Loading patch containers");

        try
        {
            bool success = await Container.LoadExistingPatchesAsync();

            if (!success)
            {
                Logger.Warn("Failed to load patch container for game {0}", Game);
                // TODO-UPDATE: Failed to load container - handle
                return;
            }

            Container.RefreshPatchedFiles();

            Logger.Info("Loaded patch container for game {0}", Game);
        }
        catch (Exception ex)
        {
            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when loading the patches");

            // TODO-UPDATE: Failed to load container - handle
        }
    }

    public async Task ApplyAsync()
    {
        // TODO-UPDATE: Set progress?
        // TODO-UPDATE: Localize
        using (await LoadOperation.RunAsync("Applying patches"))
        {
            Logger.Info("Applying patches");

            try
            {
                if (Container.HasChanges)
                    await Task.Run(() => Container.Apply());

                // TODO-UPDATE: Do we actually still want to auto-sync textures?
                //await Manager.OnRepackedArchivesAsync(Containers.Where(x => x.HasChanges).Select(x => x.ArchiveFilePath).ToArray());

                Logger.Info("Applied patches");

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplaySuccessfulActionMessageAsync("Successfully applied all patches");
            }
            catch (Exception ex)
            {
                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayExceptionMessageAsync(ex,
                    "An error occurred when applying the patches. Some patches might still have been applied.");
            }
        }
    }

    public void Dispose()
    {
        Container.Dispose();
    }

    #endregion
}