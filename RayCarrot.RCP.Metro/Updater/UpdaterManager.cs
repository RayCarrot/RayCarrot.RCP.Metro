using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Logging;
using RayCarrot.UI;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
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
            RL.Logger?.LogInformationSource($"Updates are being checked for");

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

            RL.Logger?.LogInformationSource($"The update manifest was retrieved");

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
                            RL.Logger?.LogInformationSource($"The latest version is installed");

                            // Return the result
                            return new UpdaterCheckResult();
                        }
                    }
                }

                latestFoundVersion = isBetaUpdate ? latestBetaVersion : latestVersion;

                RL.Logger?.LogInformationSource($"A new version ({latestFoundVersion}) is available");
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
        /// Updates the application
        /// </summary>
        /// <param name="result">The updater check result to use when updating</param>
        /// <param name="asAdmin">Indicates if the updater should run as admin</param>
        /// <returns>A value indicating if the operation succeeded</returns>
        public async Task<bool> UpdateAsync(UpdaterCheckResult result, bool asAdmin)
        {
            try
            {
                // Create the parent directory for the update file
                Directory.CreateDirectory(CommonPaths.UpdaterFilePath.Parent);

                // Deploy the updater
                File.WriteAllBytes(CommonPaths.UpdaterFilePath, Files.Rayman_Control_Panel_Updater);

                RL.Logger?.LogInformationSource($"The updater was created");
            }
            catch (Exception ex)
            {
                ex.HandleError("Writing updater to temp path", CommonPaths.UpdaterFilePath);

                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Update_UpdaterError, UserFallbackURL), Resources.Update_UpdaterErrorHeader);

                return false;
            }

            // Launch the updater and capture the process
            using var updateProcess = await RCFRCP.File.LaunchFileAsync(CommonPaths.UpdaterFilePath, asAdmin, 
                // Arg 1: Program path
                $"\"{Assembly.GetEntryAssembly()?.Location}\" " +
                // Arg 2: Dark mode
                $"{RCFRCP.Data.DarkMode} " +
                // Arg 3: User level
                $"{RCFRCP.Data.UserLevel} " +
                // Arg 4: Update URL
                $"\"{result.DownloadURL}\" " +
                // Arg 5: Current culture
                $"\"{RCFCore.Data.CurrentCulture}\"");

            // Make sure we have a valid process
            if (updateProcess == null)
            {
                await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.Update_RunningUpdaterError, UserFallbackURL), Resources.Update_RunningUpdaterErrorHeader, MessageType.Error);

                return false;
            }

            // Shut down the app
            await BaseRCFApp.Current.ShutdownRCFAppAsync(true);

            return true;
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
        /// The fallback URL to display to the user in case of an error
        /// </summary>
        protected abstract string UserFallbackURL { get; }

        /// <summary>
        /// The manifest URL
        /// </summary>
        protected abstract string ManifestURL { get; }
    }
}