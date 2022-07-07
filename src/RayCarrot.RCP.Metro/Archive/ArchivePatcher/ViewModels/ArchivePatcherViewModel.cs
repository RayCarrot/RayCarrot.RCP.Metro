using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace RayCarrot.RCP.Metro.Archive;

public class ArchivePatcherViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    public ArchivePatcherViewModel(IArchiveDataManager manager, IEnumerable<FileSystemPath> archiveFilePaths)
    {
        LoadOperation = new BindableOperation();

        Manager = manager;
        Containers = new ObservableCollection<PatchContainerViewModel>(
            archiveFilePaths.Select(x => new PatchContainerViewModel(new PatchContainerFile(x.AppendFileExtension(new FileExtension(PatchContainerFile.FileExtensions))), x, LoadOperation)));

        foreach (PatchContainerViewModel c in Containers)
            c.PropertyChanged += Container_OnPropertyChanged;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public IArchiveDataManager Manager { get; }
    public ObservableCollection<PatchContainerViewModel> Containers { get; }
    public PatchViewModel? SelectedPatch { get; set; }
    public bool HasPatchedFiles => Containers.Any(x => x.HasPatchedFiles);
    public bool HasChanges => Containers.Any(x => x.HasChanges);

    public BindableOperation LoadOperation { get; }

    #endregion

    #region Private Methods

    private void Container_OnPropertyChanged(object s, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PatchContainerViewModel.HasPatchedFiles))
        {
            OnPropertyChanged(nameof(HasPatchedFiles));
        }
        else if(e.PropertyName == nameof(PatchContainerViewModel.HasChanges))
        {
            OnPropertyChanged(nameof(HasChanges));
        }
        else if (e.PropertyName == nameof(PatchContainerViewModel.SelectedPatch))
        {
            var cc = (PatchContainerViewModel)s;

            if (cc.SelectedPatch is not null)
                foreach (PatchContainerViewModel container in Containers.Where(x => x != cc))
                    container.SelectedPatch = null;

            SelectedPatch = cc.SelectedPatch;
        }
    }

    #endregion

    #region Public Methods

    public void DeselectAll()
    {
        Containers.ForEach(x => x.SelectedPatch = null);
        Logger.Trace("Deselected all patches");
    }

    public async Task LoadPatchesAsync()
    {
        List<PatchContainerViewModel> failedContainers = new();

        Logger.Info("Loading patch containers");

        foreach (PatchContainerViewModel container in Containers)
        {
            try
            {
                bool success = await container.LoadExistingPatchesAsync();

                if (!success)
                {
                    Logger.Warn("Failed to load patch container {0}", container.DisplayName);
                    failedContainers.Add(container);
                    continue;
                }

                container.RefreshPatchedFiles();

                Logger.Info("Loaded patch container {0}", container.DisplayName);
            }
            catch (Exception ex)
            {
                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, $"An error occurred when loading the patches from {container.DisplayName}");

                failedContainers.Add(container);
            }
        }

        // Remove the containers which could not be loaded
        foreach (PatchContainerViewModel container in failedContainers)
        {
            Containers.Remove(container);
            container.PropertyChanged -= Container_OnPropertyChanged;
            container.Dispose();
        }

        Logger.Trace("Removed {0} containers which failed to load", failedContainers.Count);
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
                await Task.Run(() =>
                {
                    foreach (PatchContainerViewModel c in Containers.Where(x => x.HasChanges))
                        c.Apply(Manager);
                });

                await Manager.OnRepackedArchivesAsync(Containers.Where(x => x.HasChanges).Select(x => x.ArchiveFilePath).ToArray());

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
        Containers.DisposeAll();
        Manager.Dispose();
    }

    #endregion
}