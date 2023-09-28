using System.IO;

namespace RayCarrot.RCP.Metro;

public abstract class FileLaunchHandler : LaunchArgHandler
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public abstract FileAssociationInfo? FileAssociationInfo { get; }

    #endregion

    #region Public Static Methods

    public static FileLaunchHandler[] GetHandlers() => new FileLaunchHandler[]
    {
        new LegacyGamePatchFileLaunchHandler(),
        new ModFileLaunchHandler(),
    };

    public static FileLaunchHandler? GetHandler(FileSystemPath filePath) => GetHandlers().FirstOrDefault(x => x.IsValid(filePath));

    #endregion

    #region Public Methods

    public bool? IsAssociatedWithFileType()
    {
        if (FileAssociationInfo is not { } info)
            return null;

        try
        {
            return WindowsHelpers.GetFileTypeAssociationID(info.FileExtension) == info.Id;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Checking if the file type association forr {info.FileExtension} is set");
            return null;
        }
    }

    public void AssociateWithFileType(FileSystemPath programFilePath, bool enable)
    {
        if (FileAssociationInfo is not { } info)
            return;

        FileSystemPath iconFilePath = AppFilePaths.IconsPath + info.IconFileName;

        if (enable)
        {
            Directory.CreateDirectory(iconFilePath.Parent);
            using Stream fileStream = File.Create(iconFilePath);
            info.GetIconFunc().Save(fileStream);
        }
        else
        {
            Services.File.DeleteFile(iconFilePath);
        }

        WindowsHelpers.SetFileTypeAssociation(programFilePath, info.FileExtension, info.Name, info.Id, iconFilePath, enable);

        if (enable)
            Logger.Info($"Set the file type association for {info.FileExtension}");
        else
            Logger.Info($"Removed the file type association for {info.FileExtension}");
    }

    public abstract bool IsValid(FileSystemPath filePath);
    public abstract void Invoke(FileSystemPath filePath, State state);

    #endregion
}