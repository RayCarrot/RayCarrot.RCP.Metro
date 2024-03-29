﻿using System.Net;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Manages application updates
/// </summary>
public abstract class UpdaterManager : IUpdaterManager
{
    protected UpdaterManager(IMessageUIManager message, FileManager file, DeployableFilesManager deployableFiles, AppUserData data, IAppInstanceData instanceData)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        File = file ?? throw new ArgumentNullException(nameof(file));
        DeployableFiles = deployableFiles ?? throw new ArgumentNullException(nameof(deployableFiles));
        Data = data ?? throw new ArgumentNullException(nameof(data));
        InstanceData = instanceData ?? throw new ArgumentNullException(nameof(instanceData));
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected IMessageUIManager Message { get; }
    protected FileManager File { get; }
    protected DeployableFilesManager DeployableFiles { get; }
    protected AppUserData Data { get; }
    protected IAppInstanceData InstanceData { get; }

    /// <summary>
    /// Checks for updates
    /// </summary>
    /// <param name="forceUpdate">Indicates if the latest available version should be returned even if it's not newer than the current version</param>
    /// <param name="includeBeta">Indicates if beta updates should be included in the check</param>
    /// <returns>The result</returns>
    public async Task<UpdaterCheckResult> CheckAsync(bool forceUpdate, bool includeBeta)
    {
        Logger.Info("Updates are being checked for");

        string errorMessage = Resources.Update_UnknownError;
        Exception? exception = null;
        JObject? manifest = null;
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
            Logger.Warn(ex, "Getting server manifest");
            errorMessage = Resources.Update_WebError;
        }
        catch (JsonReaderException ex)
        {
            exception = ex;
            Logger.Error(ex, "Parsing server manifest");
            errorMessage = Resources.Update_FormatError;
        }
        catch (Exception ex)
        {
            exception = ex;
            Logger.Error(ex, "Getting server manifest");
            errorMessage = Resources.Update_GenericError;
        }

        // Return the error if the manifest was not retrieved
        if (manifest == null)
            return new UpdaterCheckResult(errorMessage, exception);

        // Flag indicating if the current update is a beta update
        bool isBetaUpdate = false;

        Logger.Info("The update manifest was retrieved");

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
                        Logger.Info("The latest version is installed");

                        // Return the result
                        return new UpdaterCheckResult();
                    }
                }
            }

            latestFoundVersion = isBetaUpdate ? latestBetaVersion : latestVersion;

            Logger.Info("A new version ({0}) is available", latestFoundVersion);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Getting assembly version from server manifest {0}", manifest);

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
            Logger.Error(ex, "Getting download URL from server manifest {0}", manifest);

            return new UpdaterCheckResult(Resources.Update_ManifestError, ex);
        }

        // Get the display news
        string? displayNews = null;

        try
        {
            // Get the update news
            displayNews = GetDisplayNews(manifest, isBetaUpdate);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Getting update news from server manifest {0}", manifest);
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
        FileSystemPath filePath;

        try
        {
            filePath = DeployableFiles.DeployFile(DeployableFilesManager.DeployableFile.Updater);
            Logger.Info("The updater was deployed");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Deploying updater");

            await Message.DisplayExceptionMessageAsync(ex, String.Format(Resources.Update_UpdaterError, UserFallbackURL), Resources.Update_UpdaterErrorHeader);

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
        using var updateProcess = await File.LaunchFileAsync(filePath, asAdmin, 
            // Arg 1: Program path
            $"\"{Assembly.GetEntryAssembly()?.Location}\" " +
            // Arg 2: Dark mode
            $"{Data.Theme_DarkMode} " +
            // Arg 3: User level
            $"{Data.App_UserLevel} " +
            // Arg 4: Update URL
            $"\"{result.DownloadURL}\" " +
            // Arg 5: Current culture
            $"\"{InstanceData.CurrentCulture}\" " +
            // Arg 6: Web security protocol type
            $"{webSecurityProtocolType}");

        // Make sure we have a valid process
        if (updateProcess == null)
        {
            await Message.DisplayMessageAsync(String.Format(Resources.Update_RunningUpdaterError, UserFallbackURL), Resources.Update_RunningUpdaterErrorHeader, MessageType.Error);

            return false;
        }

        // Shut down the app
        await App.Current.ShutdownAppAsync(true);

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