using RayCarrot.RCP.Metro.Patcher;

namespace RayCarrot.RCP.Metro;

public class PatchFileURILaunchHandler : URILaunchHandler
{
    public override bool DisableFullStartup => true;

    public override string BaseURI => PatchFile.URIProtocol;

    public override void Invoke(string uri, State state)
    {
        string value = GetValue(uri);

        // TODO-UPDATE: Implement
    }
}