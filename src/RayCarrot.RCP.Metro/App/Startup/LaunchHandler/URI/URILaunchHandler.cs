namespace RayCarrot.RCP.Metro;

public abstract class UriLaunchHandler : LaunchArgHandler
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public abstract string UriProtocol { get; }
    public abstract string UriProtocolName { get; }

    #endregion

    #region Protected Methods

    protected string GetValueFromUri(string uri) => uri.Substring(UriProtocol.Length + 1);

    #endregion

    #region Public Static Methods

    public static UriLaunchHandler[] GetHandlers() => new UriLaunchHandler[]
    {
        new ModFileUriLaunchHandler(),
    };

    public static UriLaunchHandler? GetHandler(string uri) => GetHandlers().FirstOrDefault(x => x.IsUriValid(uri));

    #endregion

    #region Public Methods

    public bool? IsAssociatedWithUriProtocol()
    {
        try
        {
            return WindowsHelpers.GetHasURIProtocolAssociation(UriProtocol);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Checking if the uri protocol association for {UriProtocol} is set");
            return null;
        }
    }

    public void AssociateWithUriProtocol(FileSystemPath programFilePath, bool enable)
    {
        WindowsHelpers.SetURIProtocolAssociation(programFilePath, UriProtocol, UriProtocolName, enable);

        if (enable)
            Logger.Info($"Set the uri protocol association for {UriProtocol}");
        else
            Logger.Info($"Removed the uri protocol association for {UriProtocol}");
    }

    public bool IsUriValid(string uri) => uri.StartsWith($"{UriProtocol}:", StringComparison.Ordinal);
    
    public abstract void Invoke(string uri, State state);

    #endregion
}