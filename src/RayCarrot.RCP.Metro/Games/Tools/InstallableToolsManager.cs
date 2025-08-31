namespace RayCarrot.RCP.Metro.Games.Tools;

public class InstallableToolsManager
{
    public bool CheckIsInstalled(InstallableTool tool)
    {
        return Services.Data.App_InstalledTools.ContainsKey(tool.ToolId) && 
               tool.InstallDirectory.DirectoryExists;
    }

    public Version GetInstalledVersion(InstallableTool tool)
    {
        return Services.Data.App_InstalledTools[tool.ToolId].Version;
    }

    public async Task<bool> InstallAsync(InstallableTool tool)
    {
        bool result = await Services.App.DownloadAsync(new[] { tool.DownloadUri }, true, tool.InstallDirectory);

        if (result)
        {
            Services.Data.App_InstalledTools[tool.ToolId] = new InstalledTool(
                toolId: tool.ToolId,
                path: tool.InstallDirectory,
                size: tool.InstallDirectory.GetSize(),
                downloadDateTime: DateTime.Now,
                version: tool.LatestVersion);

            Services.Messenger.Send(new ToolInstalledMessage(tool.ToolId));
        }

        return result;
    }

    public async Task UninstallAsync(InstallableTool tool)
    {
        try
        {
            Services.File.DeleteDirectory(tool.InstallDirectory);
        }
        catch (Exception ex)
        {
            // TODO-LOC
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when uninstalling the tool. Make sure it isn't currently being used and try again.");
            return;
        }

        Services.Data.App_InstalledTools.Remove(tool.ToolId);
        tool.OnUninstalled();
        Services.Messenger.Send(new ToolUninstalledMessage(tool.ToolId));
    }

    public async Task<bool> UpdateAsync(InstallableTool tool)
    {
        // Uninstall old version
        try
        {
            Services.File.DeleteDirectory(tool.InstallDirectory);
        }
        catch (Exception ex)
        {
            // TODO-LOC
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when updating the tool. Make sure it isn't currently being used and try again.");
            return false;
        }

        // Install new version
        bool result = await Services.App.DownloadAsync(new[] { tool.DownloadUri }, true, tool.InstallDirectory);

        if (result)
        {
            // Replace data for installed tool
            Services.Data.App_InstalledTools[tool.ToolId] = new InstalledTool(
                toolId: tool.ToolId,
                path: tool.InstallDirectory,
                size: tool.InstallDirectory.GetSize(),
                downloadDateTime: DateTime.Now,
                version: tool.LatestVersion);

            Services.Messenger.Send(new ToolUpdatedMessage(tool.ToolId));
        }
        else
        {
            // Finish uninstalling if updating fails
            Services.Data.App_InstalledTools.Remove(tool.ToolId);
            tool.OnUninstalled();
            Services.Messenger.Send(new ToolUninstalledMessage(tool.ToolId));
        }

        return result;
    }
}