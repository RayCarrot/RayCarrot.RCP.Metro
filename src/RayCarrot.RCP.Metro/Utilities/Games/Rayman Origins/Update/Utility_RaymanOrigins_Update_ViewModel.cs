using RayCarrot.UI;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Shell;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the Rayman Origins update utility
/// </summary>
public class Utility_RaymanOrigins_Update_ViewModel : BaseRCPViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_RaymanOrigins_Update_ViewModel()
    {
        // Create the commands
        UpdateDiscVersionCommand = new AsyncRelayCommand(UpdateDiscVersionAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand UpdateDiscVersionCommand { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Updates the disc version to the latest version (1.02)
    /// </summary>
    /// <returns>The task</returns>
    public async Task UpdateDiscVersionAsync()
    {
        try
        {
            Logger.Info("The Rayman Origins disc updater is being downloaded...");

            // Download the file
            var succeeded = await App.DownloadAsync(new Uri[]
            {
                new Uri(AppURLs.RO_Updater_URL)
            }, true, KnownFolders.Downloads.Path);

            if (succeeded)
                (await Services.File.LaunchFileAsync(Path.Combine(KnownFolders.Downloads.Path, "RaymanOriginspc_1.02.exe")))?.Dispose();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Downloading RO updater");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.ROU_UpdateFailed);
        }


    }

    #endregion
}