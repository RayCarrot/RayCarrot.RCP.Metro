using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RayCarrot.CarrotFramework.Abstractions;

namespace RayCarrot.RCP.Core
{
    /// <summary>
    /// Manages application updates
    /// </summary>
    public abstract class UpdaterManager : IUpdaterManager
    {
        /// <summary>
        /// Checks for updates
        /// </summary>
        /// <param name="forceUpdate">Indicates if the latest available version should be returned even if it's not newer than the current version</param>
        /// <param name="includeBeta">Indicates if beta updates should be included in the check</param>
        /// <returns>The result</returns>
        public async Task<UpdaterCheckResult> CheckAsync(bool forceUpdate, bool includeBeta)
        {
            RCFCore.Logger?.LogInformationSource($"Updates are being checked for");

            string errorMessage = Resources.Update_UnknownError;
            Exception exception = null;
            JObject manifest = null;
            Version latestFoundVersion;

            try
            {
                // Create the web client
                using var wc = new WebClient();

                // Download the manifest
                var result = await wc.DownloadStringTaskAsync(ManifestURL);

                // Parse the manifest
                manifest = JObject.Parse(result);
            }
            catch (WebException ex)
            {
                exception = ex;
                ex.HandleUnexpected("Getting server manifest");
                errorMessage = Resources.Update_WebError;
            }
            catch (JsonReaderException ex)
            {
                exception = ex;
                ex.HandleError("Parsing server manifest");
                errorMessage = Resources.Update_FormatError;
            }
            catch (Exception ex)
            {
                exception = ex;
                ex.HandleError("Getting server manifest");
                errorMessage = Resources.Update_GenericError;
            }

            // Return the error if the manifest was not retrieved
            if (manifest == null)
                return new UpdaterCheckResult(errorMessage, exception);

            // Flag indicating if the current update is a beta update
            bool isBetaUpdate = false;

            RCFCore.Logger?.LogInformationSource($"The update manifest was retrieved");

            try
            {
                // Get the latest version
                var latestVersion = GetLatestVersion(manifest, false);

                // Get the latest beta version
                var latestBetaVersion = GetLatestVersion(manifest, true);

                // If a new update is not available...
                if (CurrentVersion >= latestVersion)
                {
                    // If we are forcing new updates, download the latest update
                    if (forceUpdate)
                    {
                        // If we are including beta updates, check if it's newer
                        if (includeBeta)
                        {
                            // If it's newer, get the beta update
                            if (latestBetaVersion > latestVersion)
                                isBetaUpdate = true;
                        }
                    }
                    // If we are not forcing updates, check if a newer beta version is available
                    else
                    {
                        // Check if a newer beta version is available, if set to do so
                        if (includeBeta && CurrentVersion < latestBetaVersion)
                        {
                            isBetaUpdate = true;
                        }
                        else
                        {
                            RCFCore.Logger?.LogInformationSource($"The latest version is installed");

                            // Return the result
                            return new UpdaterCheckResult();
                        }
                    }
                }

                latestFoundVersion = isBetaUpdate ? latestBetaVersion : latestVersion;

                RCFCore.Logger?.LogInformationSource($"A new version ({latestFoundVersion}) is available");
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting assembly version from server manifest", manifest);

                return new UpdaterCheckResult(Resources.Update_ManifestError, ex);
            }

            // Get the download URL
            string downloadURL;

            try
            {
                downloadURL = GetDownloadURL(manifest, isBetaUpdate);
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting download URL from server manifest", manifest);

                return new UpdaterCheckResult(Resources.Update_ManifestError, ex);
            }

            // Get the display news
            string displayNews = null;

            try
            {
                // Get the update news
                displayNews = GetDisplayNews(manifest, isBetaUpdate);
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting update news from server manifest", manifest);
            }

            // Return the result
            return new UpdaterCheckResult(latestFoundVersion, downloadURL, displayNews ?? Resources.Update_NewsError, isBetaUpdate);
        }

        /// <summary>
        /// Gets the latest version from the manifest
        /// </summary>
        /// <param name="manifest">The manifest to get the value from</param>
        /// <param name="isBeta">Indicates if the update is a beta update</param>
        /// <returns>The latest version</returns>
        protected abstract Version GetLatestVersion(JObject manifest, bool isBeta);

        /// <summary>
        /// Gets the display news from the manifest
        /// </summary>
        /// <param name="manifest">The manifest to get the value from</param>
        /// <param name="isBeta">Indicates if the update is a beta update</param>
        /// <returns>The display news</returns>
        protected abstract string GetDisplayNews(JObject manifest, bool isBeta);

        /// <summary>
        /// Gets the download URL from the manifest
        /// </summary>
        /// <param name="manifest">The manifest to get the value from</param>
        /// <param name="isBeta">Indicates if the update is a beta update</param>
        /// <returns>The download URL</returns>
        protected abstract string GetDownloadURL(JObject manifest, bool isBeta);

        /// <summary>
        /// The current version of the application
        /// </summary>
        protected abstract Version CurrentVersion { get; }

        /// <summary>
        /// The manifest URL
        /// </summary>
        protected abstract string ManifestURL { get; }
    }
}