using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro.Archive;

/*
 
This is a work in process Archive Patcher system. It works by creating an .apc (Archive Patch Container) file next to the archive
file which keeps track of all patches and the modified file history to then allow it to be restored.
The way a container works is this:
- At the root we have a manifest file. It has the history (which files have been modified) and a list of patches.
- Each patch has an ID (a GUID) which identifies it. It stores its resources in a folder matching the ID. The patch resources are 
  always lower-case and use forward slashes so that they can be normalized. The correct character casing is stored in the patch
  manifest.

TODO:
- Create patches for UbiRay in Legends and fixing transparency on Whale Bay texture in Rayman 2
- Add logs for how each file is modified for easier testing
- When opening the Archive Patcher we want to check if the checksums for the modified files match, if not show a warning icon next to
  the patch and show which files don't match in the info panel. This means the files have been manually modified after applying 
  the patch.
 
 */


public class ArchivePatcherViewModel : BaseViewModel, IDisposable
{
    public ArchivePatcherViewModel(IArchiveDataManager manager, IEnumerable<FileSystemPath> archiveFilePaths)
    {
        LoadOperation = new BindableOperation();

        Manager = manager;
        Containers = new ObservableCollection<PatchContainerViewModel>(
            archiveFilePaths.Select(x => new PatchContainerViewModel(new PatchContainerFile(x.AppendFileExtension(new FileExtension(PatchContainerFile.FileExtensions))), x, LoadOperation)));

        foreach (PatchContainerViewModel c in Containers)
            c.PropertyChanged += Container_OnPropertyChanged;
    }

    public IArchiveDataManager Manager { get; }
    public ObservableCollection<PatchContainerViewModel> Containers { get; }
    public PatchViewModel? SelectedPatch { get; set; }
    public bool HasPatchedFiles => Containers.Any(x => x.HasPatchedFiles);

    public BindableOperation LoadOperation { get; }

    private void Container_OnPropertyChanged(object s, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PatchContainerViewModel.HasPatchedFiles))
        {
            OnPropertyChanged(nameof(HasPatchedFiles));
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

    public void DeselectAll()
    {
        Containers.ForEach(x => x.SelectedPatch = null);
    }

    public async Task LoadPatchesAsync()
    {
        List<PatchContainerViewModel> failedContainers = new();

        foreach (PatchContainerViewModel container in Containers)
        {
            try
            {
                bool success = await container.LoadExistingPatchesAsync();

                if (!success)
                {
                    failedContainers.Add(container);
                    continue;
                }

                container.RefreshPatchedFiles();
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
    }

    public async Task ApplyAsync()
    {
        // TODO-UPDATE: Set progress?
        // TODO-UPDATE: Localize
        using (await LoadOperation.RunAsync("Applying patches"))
        {
            // TODO-UPDATE: Log

            try
            {
                await Task.Run(() =>
                {
                    foreach (PatchContainerViewModel c in Containers)
                        c.Apply(Manager);
                });

                await Manager.OnRepackedArchivesAsync(Containers.Select(x => x.ArchiveFilePath).ToArray());

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
}