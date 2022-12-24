namespace RayCarrot.RCP.Metro.Games.Components;

public class RemoveAddedFilesOnGameRemovedComponent : OnGameRemovedComponent
{
    public RemoveAddedFilesOnGameRemovedComponent() : base(RemoveAddedFiles) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static void RemoveAddedFiles(GameInstallation gameInstallation)
    {
        AddedGameFiles? addedGameFiles = gameInstallation.GetObject<AddedGameFiles>(GameDataKey.RCP_AddedFiles);

        if (addedGameFiles == null)
            return;

        foreach (FileSystemPath filePath in addedGameFiles.Files)
        {
            try
            {
                // Remove the file
                Services.File.DeleteFile(filePath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Removing added game file");
            }
        }
    }
}