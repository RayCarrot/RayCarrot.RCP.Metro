#nullable disable
using System;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the Rayman Fiesta Run options
/// </summary>
public class GameOptions_FiestaRun_ViewModel : BaseRCPViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public GameOptions_FiestaRun_ViewModel()
    {
        // Create properties
        AsyncLock = new AsyncLock();

        // Get the manager
        var manager = Games.RaymanFiestaRun.GetManager<GameManager_RaymanFiestaRun_WinStore>(GameType.WinStore);

        // Get the install directories for each version
        DefaultInstallDir = manager.GetPackageInstallDirectory(manager.GetFiestaRunPackageName(UserData_FiestaRunEdition.Default));
        PreloadInstallDir = manager.GetPackageInstallDirectory(manager.GetFiestaRunPackageName(UserData_FiestaRunEdition.Preload));
        Win10InstallDir = manager.GetPackageInstallDirectory(manager.GetFiestaRunPackageName(UserData_FiestaRunEdition.Win10));

        // Get the current version
        _selectedFiestaRunVersion = Data.Game_FiestaRunVersion;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private UserData_FiestaRunEdition _selectedFiestaRunVersion;

    #endregion

    #region Private Properties

    /// <summary>
    /// The install directory for <see cref="UserData_FiestaRunEdition.Default"/>
    /// </summary>
    private string DefaultInstallDir { get; }

    /// <summary>
    /// The install directory for <see cref="UserData_FiestaRunEdition.Preload"/>
    /// </summary>
    private string PreloadInstallDir { get; }

    /// <summary>
    /// The install directory for <see cref="UserData_FiestaRunEdition.Win10"/>
    /// </summary>
    private string Win10InstallDir { get; }

    /// <summary>
    /// The async lock for <see cref="UpdateFiestaRunVersionAsync"/>
    /// </summary>
    private AsyncLock AsyncLock { get; }

    #endregion

    #region Public Properties

    /// <summary>
    /// Indicates if <see cref="UserData_FiestaRunEdition.Default"/> is available
    /// </summary>
    public bool IsFiestaRunDefaultAvailable => DefaultInstallDir != null;

    /// <summary>
    /// Indicates if <see cref="UserData_FiestaRunEdition.Preload"/> is available
    /// </summary>
    public bool IsFiestaRunPreloadAvailable => PreloadInstallDir != null;

    /// <summary>
    /// Indicates if <see cref="UserData_FiestaRunEdition.Win10"/> is available
    /// </summary>
    public bool IsFiestaRunWin10Available => Win10InstallDir != null;

    /// <summary>
    /// The selected Fiesta Run version
    /// </summary>
    public UserData_FiestaRunEdition SelectedFiestaRunVersion
    {
        get => _selectedFiestaRunVersion;
        set
        {
            // Update value
            _selectedFiestaRunVersion = value;

            // Update data
            Task.Run(UpdateFiestaRunVersionAsync);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Updates the current Fiesta Run version based on the selected one
    /// </summary>
    /// <returns>The task</returns>
    public async Task UpdateFiestaRunVersionAsync()
    {
        using (await AsyncLock.LockAsync())
        {
            try
            {
                // Update the version
                Data.Game_FiestaRunVersion = SelectedFiestaRunVersion;

                // Update the install location
                FileSystemPath installLocation = SelectedFiestaRunVersion switch
                {
                    UserData_FiestaRunEdition.Default => DefaultInstallDir,
                    UserData_FiestaRunEdition.Preload => PreloadInstallDir,
                    UserData_FiestaRunEdition.Win10 => Win10InstallDir,
                    _ => throw new ArgumentOutOfRangeException(nameof(SelectedFiestaRunVersion),
                        SelectedFiestaRunVersion, null)
                };

                // TODO-14: Fix this
                var gameInstallation = Games.RaymanFiestaRun.GetInstallation();
                Services.Data.Game_GameInstallations.Remove(gameInstallation);
                // TODO-14: Copy over additional data as well
                Services.Data.Game_GameInstallations.Add(new GameInstallation(gameInstallation.Id, gameInstallation.GameType, installLocation, gameInstallation.IsRCPInstalled));

                await Services.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Games.RaymanFiestaRun.GetInstallation(), RefreshFlags.GameInfo));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Updating Fiesta Run install directory");
            }
        }
    }

    #endregion
}