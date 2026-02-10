using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Octokit;

namespace RayCarrot.RCP.Metro;

public class UpdaterManager : IUpdaterManager
{
    public UpdaterManager(
        IHttpClientFactory httpClientFactory,
        IMessageUIManager message, 
        FileManager file, 
        DeployableFilesManager deployableFiles, 
        AppUserData data, 
        IAppInstanceData instanceData)
    {
        HttpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        File = file ?? throw new ArgumentNullException(nameof(file));
        DeployableFiles = deployableFiles ?? throw new ArgumentNullException(nameof(deployableFiles));
        Data = data ?? throw new ArgumentNullException(nameof(data));
        InstanceData = instanceData ?? throw new ArgumentNullException(nameof(instanceData));
    }

    private const string GitHubUserName = "RayCarrot";
    private const string GitHubRepoName = "RayCarrot.RCP.Metro";
    private const string ExeFileName = "RaymanControlPanel.exe";
    private const string ChangelogFileName = "Changelog.txt";
    private const string FallbackUrl = AppURLs.LatestGitHubReleaseUrl;

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private IHttpClientFactory HttpClientFactory { get; }
    private IMessageUIManager Message { get; }
    private FileManager File { get; }
    private DeployableFilesManager DeployableFiles { get; }
    private AppUserData Data { get; }
    private IAppInstanceData InstanceData { get; }

    private static async Task<Release> GetLatestReleaseAsync(GitHubClient client, bool includePreRelease)
    {
        if (includePreRelease)
        {
            // Get latest release no matter if it's a pre-release or not
            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll(GitHubUserName, GitHubRepoName, new ApiOptions
            {
                StartPage = 1,
                PageCount = 1,
                PageSize = 1
            });
            return releases[0];
        }
        else
        {
            // Get the latest release (does not include pre-releases)
            return await client.Repository.Release.GetLatest(GitHubUserName, GitHubRepoName);
        }
    }

    private static async Task<string> GetChangelogAsync(HttpClient httpClient, Release release)
    {
        // Attempt to get the changelog from file
        if (release.Assets.FirstOrDefault(x => x.Name == ChangelogFileName) is { } changelogAsset)
        {
            return await httpClient.GetStringAsync(changelogAsset.BrowserDownloadUrl);
        }
        // Fall back to use the release description
        else
        {
            return release.Body;
        }
    }

    public async Task<UpdaterCheckResult> CheckAsync(bool forceUpdate, bool includeBeta)
    {
        Logger.Info("Checking for app updates");

        try
        {
            // Create the github client
            GitHubClient client = new(new ProductHeaderValue("RaymanControlPanel", AppViewModel.AppVersion.ToString()));

            // Get the latest release
            Release latestRelease = await GetLatestReleaseAsync(client, includeBeta);

            Logger.Info("Found latest release as {0}", latestRelease.TagName);

            // Get the asset which has the exe file
            ReleaseAsset? exeAsset = latestRelease.Assets.FirstOrDefault(x => x.Name == ExeFileName);
            if (exeAsset == null)
            {
                Logger.Warn("Latest release has no matching exe file. Attempting to find from earlier releases...");

                // If not found in the latest release then the update system might have changed. We want to go back and find the last valid release.
                IReadOnlyList<Release> allReleases = await client.Repository.Release.GetAll(GitHubUserName, GitHubRepoName);
                foreach (Release release in allReleases)
                {
                    exeAsset = latestRelease.Assets.FirstOrDefault(x => x.Name == ExeFileName);
                    if (exeAsset != null)
                    {
                        latestRelease = release;
                        break;
                    }
                }

                // If the exe asset is still not found then we can't update
                if (exeAsset == null)
                {
                    Logger.Error("Could not find a release with a valid exe file");
                    return new UpdaterCheckResult("No valid app version found", null); // TODO-LOC
                }
             
                Logger.Info("Found latest release with a valid exe file as {0}", latestRelease.TagName);
            }

            // Get the version from the tag name
            Version latestVersion = Version.Parse(latestRelease.TagName);

            // Check if it's a new version
            if (latestVersion <= AppViewModel.AppVersion && !forceUpdate)
            {
                // No new update available
                Logger.Info("The latest version is installed");
                return new UpdaterCheckResult();
            }

            Logger.Info("A new version ({0}) is available", latestVersion);

            // Get the changelog from the release
            using HttpClient httpClient = HttpClientFactory.CreateClient();
            string changelog = await GetChangelogAsync(httpClient, latestRelease);

            return new UpdaterCheckResult(latestVersion, exeAsset.BrowserDownloadUrl, exeAsset.Size, latestRelease.CreatedAt, changelog, latestRelease.Prerelease);
        }
        catch (HttpRequestException ex)
        {
            Logger.Error(ex, "Checking for updates");
            return new UpdaterCheckResult("Unable to connect to the server. Please check your internet connection.", ex); // TODO-LOC
        }
        catch (WebException ex)
        {
            Logger.Error(ex, "Checking for updates");
            return new UpdaterCheckResult("Unable to connect to the server. Please check your internet connection.", ex); // TODO-LOC
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Checking for updates");
            return new UpdaterCheckResult("An unknown error occurred", ex); // TODO-LOC
        }
    }

    public async Task<bool> UpdateAsync(UpdaterCheckResult result, bool asAdmin)
    {
        FileSystemPath filePath;

        try
        {
            filePath = DeployableFiles.DeployFile(DeployableFilesManager.DeployableFile.Updater);
            Logger.Info("The updater was deployed");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Deploying updater");

            await Message.DisplayExceptionMessageAsync(ex, String.Format(Resources.Update_UpdaterError, FallbackUrl), Resources.Update_UpdaterErrorHeader);

            return false;
        }

        int webSecurityProtocolType = 0;

        try
        {
            webSecurityProtocolType = (int)ServicePointManager.SecurityProtocol;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to get current web security protocol");
        }

        // Launch the updater and capture the process
        using Process? updateProcess = await File.LaunchFileAsync(filePath, asAdmin, 
            // Arg 1: Program path
            $"\"{Assembly.GetEntryAssembly()?.Location}\" " +
            // Arg 2: Dark mode
            $"{Data.Theme_DarkMode} " +
            // Arg 3: User level
            $"{Data.App_UserLevel} " +
            // Arg 4: Update URL
            $"\"{result.NewVersionUrl}\" " +
            // Arg 5: Current culture
            $"\"{InstanceData.CurrentCulture}\" " +
            // Arg 6: Web security protocol type
            $"{webSecurityProtocolType}");

        // Make sure we have a valid process
        if (updateProcess == null)
        {
            await Message.DisplayMessageAsync(String.Format(Resources.Update_RunningUpdaterError, FallbackUrl), Resources.Update_RunningUpdaterErrorHeader, MessageType.Error);

            return false;
        }

        // Shut down the app
        await App.Current.ShutdownAppAsync(true);

        return true;
    }
}