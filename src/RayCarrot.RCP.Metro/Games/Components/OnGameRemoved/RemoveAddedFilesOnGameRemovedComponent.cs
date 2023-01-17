using RayCarrot.RCP.Metro.Games.Data;

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

        Logger.Info("Removing added game files");

        foreach (FileSystemPath filePath in addedGameFiles.Files)
        {
            try
            {
                // Remove the file
                Services.File.DeleteFile(filePath);

                Logger.Trace("Removed added file {0}", filePath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Removing added game file");
            }
        }
    }
}