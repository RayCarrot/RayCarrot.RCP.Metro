using System.Windows.Input;
using BinarySerializer;

namespace RayCarrot.RCP.Metro.ModLoader.Modules.Deltas;

public class DeltasModuleViewModel : ModModuleViewModel
{
    public DeltasModuleViewModel(ModModule module, bool isSingleFile) : base(module)
    {
        IsSingleFile = isSingleFile;
        CreateDeltaFileCommand = new AsyncRelayCommand(CreateDeltaFileAsync);
    }

    public ICommand CreateDeltaFileCommand { get; }

    public bool IsSingleFile { get; }

    public async Task CreateDeltaFileAsync()
    {
        FileBrowserResult originalFileBrowseResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel
        {
            Title = Resources.ModLoader_DeltasModule_BrowseOriginalFile,
        });

        if (originalFileBrowseResult.CanceledByUser)
            return;

        FileBrowserResult modifiedFileBrowseResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel
        {
            Title = Resources.ModLoader_DeltasModule_BrowseModifiedFile,
        });

        if (modifiedFileBrowseResult.CanceledByUser)
            return;

        DeltaFile deltaFile = DeltaFile.Create(originalFileBrowseResult.SelectedFile, modifiedFileBrowseResult.SelectedFile);

        if (deltaFile.Chunks.Length == 0)
        {
            await Services.MessageUI.DisplayMessageAsync(Resources.ModLoader_DeltasModule_IdenticalFilesError, Resources.ModLoader_DeltasModule_IdenticalFilesErrorHeader, MessageType.Warning);
            return;
        }
        
        FileSystemPath deltaFilePath = IsSingleFile 
            ? modifiedFileBrowseResult.SelectedFile.Parent + DeltasModule.SingleFileDeltaPatchName 
            : modifiedFileBrowseResult.SelectedFile.AppendFileExtension(new FileExtension(DeltasModule.FileExtension));

        using Context context = new RCPContext(deltaFilePath.Parent);
        context.WriteFileData(deltaFilePath.Name, deltaFile);

        await Services.File.OpenExplorerLocationAsync(deltaFilePath);
    }
}