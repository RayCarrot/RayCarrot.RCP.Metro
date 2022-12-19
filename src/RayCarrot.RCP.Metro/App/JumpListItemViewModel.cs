using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a jump list item
/// </summary>
public class JumpListItemViewModel : BaseRCPViewModel
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="name">The item name</param>
    /// <param name="iconSource">The item icon resource path</param>
    /// <param name="launchPath">The item launch path</param>
    /// <param name="workingDirectory">The working directory for the launch path</param>
    /// <param name="launchArguments">The item launch arguments</param>
    /// <param name="id">The item ID</param>
    public JumpListItemViewModel(LocalizedString name, string? iconSource, string launchPath, string? workingDirectory, string? launchArguments, string id)
    {
        Name = name;
        IconSource = iconSource;
        LaunchPath = launchPath;
        WorkingDirectory = workingDirectory;
        LaunchArguments = launchArguments;
        ID = id;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Sets the icon image source
    /// </summary>
    public void SetIconImageSource()
    {
        try
        {
            IconImageSource = IconSource == null ? null : WindowsHelpers.GetIconOrThumbnail(IconSource, ShellThumbnailSize.Small).ToImageSource();
            IconImageSource?.Freeze();
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Getting jump list icon image source");
        }
    }

    /// <summary>
    /// The item name
    /// </summary>
    public LocalizedString Name { get; }

    /// <summary>
    /// The item icon resource path
    /// </summary>
    public string? IconSource { get; }

    /// <summary>
    /// The item icon image source
    /// </summary>
    public ImageSource? IconImageSource { get; set; }

    /// <summary>
    /// The item launch path
    /// </summary>
    public string LaunchPath { get; }

    /// <summary>
    /// The working directory for the launch path
    /// </summary>
    public string? WorkingDirectory { get; }

    /// <summary>
    /// The item launch arguments
    /// </summary>
    public string? LaunchArguments { get; }

    /// <summary>
    /// The item ID
    /// </summary>
    public string ID { get; } // TODO-14: Update how this works
}