namespace RayCarrot.RCP.Metro;

public class AssociatedFileEditorsManager
{
    public AssociatedFileEditorsManager(AppUserData data, IMessenger messenger)
    {
        Data = data;
        Messenger = messenger;
    }

    private AppUserData Data { get; }
    private IMessenger Messenger { get; }

    public string BinaryFileExtension => String.Empty;

    public Dictionary<string, FileSystemPath> GetFileEditorAssociations() => Data.FileEditors_AssociatedEditors;

    public void AddFileEditorAssociaton(string fileExtension, FileSystemPath programFilePath)
    {
        Data.FileEditors_AssociatedEditors.Add(fileExtension, programFilePath);
        Messenger.Send(new FileEditorAssociationAdded(fileExtension, programFilePath));
    }

    public void RemoveFileEditorAssociaton(string fileExtension)
    {
        Data.FileEditors_AssociatedEditors.Remove(fileExtension);
        Messenger.Send(new FileEditorAssociationRemoved(fileExtension));
    }

    public FileSystemPath? GetFileEditorAssociaton(string fileExtension)
    {
        return Data.FileEditors_AssociatedEditors.TryGetValue(fileExtension, out FileSystemPath path) ? path : null;
    }

    public async Task<FileSystemPath?> RequestFileEditorAssociatonAsync(string fileExtension)
    {
        // Start by checking if a file editor already is associated
        FileSystemPath? editorPath = GetFileEditorAssociaton(fileExtension);
        if (editorPath != null)
        {
            // Make sure the file still exists
            if (editorPath.Value.FileExists)
                return editorPath.Value;

            // Remove if it doesn't exist
            RemoveFileEditorAssociaton(fileExtension);
        }

        bool isBinary = fileExtension == BinaryFileExtension;
        string extName = isBinary ? Resources.Archive_EditBinary.ToLower() : new FileExtension(fileExtension).ToString();

        ProgramSelectionResult programResult = await Services.UI.GetProgramAsync(new ProgramSelectionViewModel()
        {
            Title = String.Format(Resources.Archive_SelectEditExe, extName),
            FileExtensions = isBinary ? null : new[] { new FileExtension(fileExtension) },
        });

        if (programResult.CanceledByUser)
            return null;

        AddFileEditorAssociaton(fileExtension, programResult.ProgramFilePath);
        return programResult.ProgramFilePath;
    }

    public void UpdateFileEditorAssociaton(string fileExtension, FileSystemPath programFilePath)
    {
        Data.FileEditors_AssociatedEditors[fileExtension] = programFilePath;
    }
}