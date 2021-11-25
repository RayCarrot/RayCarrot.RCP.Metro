#nullable disable
namespace RayCarrot.RCP.Metro;

/// <summary>
/// Shortcuts for the common application services
/// </summary>
public static class Services
{
    /// <summary>
    /// Gets the common app data
    /// </summary>
    public static IAppInstanceData InstanceData => BaseApp.Current.GetService<IAppInstanceData>();

    /// <summary>
    /// Gets the message UIManager
    /// </summary>
    public static IMessageUIManager MessageUI => BaseApp.Current.GetService<IMessageUIManager>();

    /// <summary>
    /// Gets the browse UIManager
    /// </summary>
    public static IBrowseUIManager BrowseUI => BaseApp.Current.GetService<IBrowseUIManager>();

    /// <summary>
    /// Gets the dialog base manager, or the default one
    /// </summary>
    public static IDialogBaseManager DialogBaseManager => BaseApp.Current.GetService<IDialogBaseManager>();

    /// <summary>
    /// The application user data
    /// </summary>
    public static AppUserData Data => BaseApp.Current.GetService<AppUserData>();

    /// <summary>
    /// The app view model
    /// </summary>
    public static AppViewModel App => BaseApp.Current.GetService<AppViewModel>();

    /// <summary>
    /// The App UI manager
    /// </summary>
    public static AppUIManager UI => BaseApp.Current.GetService<AppUIManager>();

    /// <summary>
    /// The backup manager
    /// </summary>
    public static GameBackups_Manager Backup => BaseApp.Current.GetService<GameBackups_Manager>();

    /// <summary>
    /// The update manager
    /// </summary>
    public static IUpdaterManager UpdaterManager => BaseApp.Current.GetService<IUpdaterManager>();

    /// <summary>
    /// The file manager
    /// </summary>
    public static IFileManager File => BaseApp.Current.GetService<IFileManager>();
}