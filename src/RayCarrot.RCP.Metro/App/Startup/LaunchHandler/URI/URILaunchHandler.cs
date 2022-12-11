namespace RayCarrot.RCP.Metro;

public abstract class URILaunchHandler : LaunchArgHandler
{
    public static URILaunchHandler[] Handlers => new URILaunchHandler[]
    {
        new PatchFileURILaunchHandler(),
    };

    public static URILaunchHandler? GetHandler(string uri) => Handlers.FirstOrDefault(x => x.IsValid(uri));

    public abstract string BaseURI { get; }

    protected string GetValue(string uri) => uri.Substring(BaseURI.Length + 1);

    public bool IsValid(string uri) => uri.StartsWith($"{BaseURI}:", StringComparison.Ordinal);
    public abstract void Invoke(string uri, State state);
}