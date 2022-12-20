using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a jump list item
/// </summary>
public class JumpListItemViewModel : BaseRCPViewModel, IComparable<JumpListItemViewModel>
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="gameInstallation">The game installation the item is for</param>
    /// <param name="name">The item name</param>
    /// <param name="iconSource">The item icon resource path</param>
    /// <param name="launchPath">The item launch path</param>
    /// <param name="workingDirectory">The working directory for the launch path</param>
    /// <param name="launchArguments">The item launch arguments</param>
    /// <param name="id">The item ID</param>
    public JumpListItemViewModel(GameInstallation gameInstallation, LocalizedString name, string? iconSource, string launchPath, string? workingDirectory, string? launchArguments, string id)
    {
        GameInstallation = gameInstallation;
        Name = name;
        IconSource = iconSource;
        LaunchPath = launchPath;
        WorkingDirectory = workingDirectory;
        LaunchArguments = launchArguments;
        Id = id;
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
    /// The game installation the item is for
    /// </summary>
    public GameInstallation GameInstallation { get; }

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
    public string Id { get; }

    public int CompareTo(JumpListItemViewModel? other)
    {
        if (other == this) 
            return 0;

        if (other == null) 
            return 1;

        return GameInstallation.CompareTo(other.GameInstallation);
    }
}