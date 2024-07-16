namespace RayCarrot.RCP.Metro.Ini;

public class UbisoftIniAppDataManager
{
    public UbisoftIniAppDataManager(IniAppData appData, FileSystemPath appDataFilePath)
    {
        AppData = appData;
        AppDataFilePath = appDataFilePath;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public IniAppData AppData { get; }
    public FileSystemPath AppDataFilePath { get; }

    /// <summary>
    /// Enables write access to the ubi.ini file
    /// </summary>
    public async Task EnableUbiIniWriteAccessAsync()
    {
        // Due to the ubi.ini file being located in the C:\Windows directory you normally don't have
        // access to it unless running a process as admin. To avoid having to always run RCP as admin
        // we have the user accept a one-time admin prompt which gives everyone full access to the
        // ubi.ini file. Previously this was done during the RCP app startup, but has now been moved
        // here so that it only prompts the user when they are to make a change to the file.

        try
        {
            if (!AppDataFilePath.FileExists)
            {
                Logger.Info("The ubi.ini file was not found");
                return;
            }

            // Check if we have write access
            if (Services.File.CheckFileWriteAccess(AppDataFilePath))
            {
                Logger.Debug("The ubi.ini file has write access");
                return;
            }

            await Services.MessageUI.DisplayMessageAsync(Resources.UbiIniWriteAccess_InfoMessage);

            // Attempt to change the permission
            await Services.App.RunAdminWorkerAsync(AdminWorker.GrantFullFileControlArg, AppDataFilePath);

            Logger.Info("The ubi.ini file permission was changed");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Changing ubi.ini file permissions");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.UbiIniWriteAccess_Error);
        }
    }

    /// <summary>
    /// Loads the app data
    /// </summary>
    public void Load()
    {
        AppData.Load(AppDataFilePath);
        Logger.Info("The Ubisoft app data has been loaded");
    }

    /// <summary>
    /// Saves the app data
    /// </summary>
    public void Save()
    {
        AppData.Save(AppDataFilePath);
        Logger.Info("The Ubisoft app data has been saved");
    }
}

public class UbisoftIniAppDataManager<T> : UbisoftIniAppDataManager
    where T : IniAppData, new()
{
    public UbisoftIniAppDataManager(FileSystemPath appDataFilePath) : base(new T(), appDataFilePath) { }

    public new T AppData => (T)base.AppData;
}