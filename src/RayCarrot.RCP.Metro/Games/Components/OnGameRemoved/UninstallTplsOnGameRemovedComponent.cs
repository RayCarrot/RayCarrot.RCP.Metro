using RayCarrot.RCP.Metro.Games.Data;

namespace RayCarrot.RCP.Metro.Games.Components;

public class UninstallTplsOnGameRemovedComponent : OnGameRemovedComponent
{
    public UninstallTplsOnGameRemovedComponent() : base(UninstallTplsAsync) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static async Task UninstallTplsAsync(GameInstallation gameInstallation)
    {
        Rayman1TplsData? data = gameInstallation.GetObject<Rayman1TplsData>(GameDataKey.R1_TplsData);

        // Return if not installed
        if (data == null)
            return;

        try
        {
            Services.File.DeleteDirectory(data.InstallDir);
            gameInstallation.SetObject<Rayman1TplsData>(GameDataKey.R1_TplsData, null);

            if (data.GameClientInstallationId != null)
                await Services.GameClients.RemoveGameClientAsync(data.GameClientInstallationId);

            Logger.Info("The TPLS utility has been uninstalled");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Uninstalling TPLS");

            // TODO-UPDATE: Update string to clarify what is being uninstalled
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.R1U_TPLSUninstallError, Resources.R1U_TPLSUninstallErrorHeader);
        }
    }
}