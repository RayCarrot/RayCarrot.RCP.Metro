#nullable disable
using Newtonsoft.Json.Linq;
using System;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The update manager for Rayman Control Panel
/// </summary>
public class RCPUpdaterManager : UpdaterManager
{
    /// <summary>
    /// Gets the latest version from the manifest
    /// </summary>
    /// <param name="manifest">The manifest to get the value from</param>
    /// <param name="isBeta">Indicates if the update is a beta update</param>
    /// <returns>The latest version</returns>
    protected override Version GetLatestVersion(JObject manifest, bool isBeta)
    {
        // Get the latest version
        var version = manifest[isBeta ? "LatestBetaVersion" : "LatestAssemblyVersion"];

        return new Version(version["Major"].Value<int>(), version["Minor"].Value<int>(), version["Build"].Value<int>(), version["Revision"].Value<int>());
    }

    /// <summary>
    /// Gets the display news from the manifest
    /// </summary>
    /// <param name="manifest">The manifest to get the value from</param>
    /// <param name="isBeta">Indicates if the update is a beta update</param>
    /// <returns>The display news</returns>
    protected override string GetDisplayNews(JObject manifest, bool isBeta)
    {
        return manifest[isBeta ? "BetaDisplayNews" : "DisplayNews"].Value<string>();
    }

    /// <summary>
    /// Gets the download URL from the manifest
    /// </summary>
    /// <param name="manifest">The manifest to get the value from</param>
    /// <param name="isBeta">Indicates if the update is a beta update</param>
    /// <returns>The download URL</returns>
    protected override string GetDownloadURL(JObject manifest, bool isBeta)
    {
        return manifest[isBeta ? "BetaURL" : "URL"].Value<string>();
    }

    /// <summary>
    /// The current version of the application
    /// </summary>
    protected override Version CurrentVersion => Services.App.CurrentAppVersion;

    /// <summary>
    /// The fallback URL to display to the user in case of an error
    /// </summary>
    protected override string UserFallbackURL => "https://raym.app/rcp/";

    /// <summary>
    /// The manifest URL
    /// </summary>
    protected override string ManifestURL => AppURLs.UpdateManifestUrl;
}