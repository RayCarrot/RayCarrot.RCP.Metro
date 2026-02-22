using Octokit;

namespace RayCarrot.RCP.Metro;

public class RCPGitHubClient : GitHubClient
{
    public RCPGitHubClient() : base(new ProductHeaderValue("RaymanControlPanel", AppViewModel.AppVersion.ToString())) { }
}