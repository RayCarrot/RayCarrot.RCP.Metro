#nullable disable
namespace RayCarrot.RCP.Metro;

// TODO: Clean up DI implementation in app:
//       - Get rid of all references to this static class. In most cases we want to get the services from the ctor.
//       - Refactor interfaces, maybe get rid of them until they're needed? Their abstraction isn't always helpful.
//       - Get rid of BaseRCPViewModel and its service caching. Better to have view models be transients in the DI container.

/// <summary>
/// Shortcuts for the common application services
/// </summary>
public static class Services
{
    /// <summary>
    /// Gets the common app data
    /// </summary>
    public static IAppInstanceData InstanceData => Metro.App.Current.GetService<IAppInstanceData>();

    /// <summary>
    /// Gets the message UIManager
    /// </summary>
    public static IMessageUIManager MessageUI => Metro.App.Current.GetService<IMessageUIManager>();

    /// <summary>
    /// Gets the browse UIManager
    /// </summary>
    public static IBrowseUIManager BrowseUI => Metro.App.Current.GetService<IBrowseUIManager>();

    /// <summary>
    /// Gets the dialog base manager, or the default one
    /// </summary>
    public static IDialogBaseManager DialogBaseManager => Metro.App.Current.GetService<IDialogBaseManager>();

    /// <summary>
    /// The application user data
    /// </summary>
    public static AppUserData Data => Metro.App.Current.GetService<AppUserData>();

    /// <summary>
    /// The app view model
    /// </summary>
    public static AppViewModel App => Metro.App.Current.GetService<AppViewModel>();

    /// <summary>
    /// The App UI manager
    /// </summary>
    public static AppUIManager UI => Metro.App.Current.GetService<AppUIManager>();

    /// <summary>
    /// The backup manager
    /// </summary>
    public static GameBackups_Manager Backup => Metro.App.Current.GetService<GameBackups_Manager>();

    /// <summary>
    /// The file manager
    /// </summary>
    public static IFileManager File => Metro.App.Current.GetService<IFileManager>();
}