using System.IO;

namespace RayCarrot.RCP.Metro.Patcher;

// TODO: Merge with Patcher?

/// <summary>
/// A game patch library. This is stored in the root of a game installation as a hidden folder and keeps track of the
/// applied patches and the original files which have been replaced (history) so that they can be restored.
/// </summary>
public class PatchLibrary
{
    #region Constructor

    public PatchLibrary(FileSystemPath gameDir, FileManager fileManager)
    {
        DirectoryPath = gameDir + ".patches";
        FileManager = fileManager;
        LibraryFilePath = DirectoryPath + LibraryFileName;
    }

    #endregion

    #region Public Properties

    public string LibraryFileName => $"library{PatchLibraryFile.FileExtension}";
    public FileSystemPath DirectoryPath { get; }
    public FileSystemPath LibraryFilePath { get; }

    #endregion

    #region Services

    private FileManager FileManager { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Ensures the library is set up correctly. Call this before modifying the library.
    /// </summary>
    public void Setup()
    {
        DirectoryInfo dirInfo = new(DirectoryPath);
        dirInfo.Create();
        dirInfo.Attributes |= FileAttributes.Hidden;
    }

    public string GetPatchFileName(string patchID) => $"{patchID}{PatchFile.FileExtension}";
    public FileSystemPath GetPatchFilePath(string patchID) => DirectoryPath + GetPatchFileName(patchID);

    public void AddPatch(FileSystemPath patchFile, string patchID, bool move)
    {
        FileSystemPath patchPath = GetPatchFilePath(patchID);

        if (move)
            FileManager.MoveFile(patchFile, patchPath, true);
        else
            FileManager.CopyFile(patchFile, patchPath, true);
    }
    public void RemovePatch(string patchID) => FileManager.DeleteFile(GetPatchFilePath(patchID));

    #endregion
}