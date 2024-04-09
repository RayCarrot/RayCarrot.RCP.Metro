#nullable disable
using Microsoft.WindowsAPICodePack.Shell;
using Nito.AsyncEx;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a drive selection
/// </summary>
public class DriveSelectionViewModel : BaseViewModel
{
    #region Constructor

    /// <summary>
    /// Creates a new instance of <see cref="DriveSelectionViewModel"/> with default values
    /// </summary>
    public DriveSelectionViewModel()
    {
        RefreshAsyncLock = new AsyncLock();

        BrowseVM = new DriveBrowserViewModel()
        {
            AllowedTypes = new DriveType[]
            {
                DriveType.CDRom,
                DriveType.Fixed,
                DriveType.Network,
                DriveType.Removable
            },
            AllowNonReadyDrives = false,
            MultiSelection = false,
            Title = Resources.Browse_SelectDrive
        };
        Setup();
        Drives = new ObservableCollection<DriveViewModel>();
    }

    /// <summary>
    /// Creates a new instance of <see cref="DriveSelectionViewModel"/> with a browse view model
    /// </summary>
    public DriveSelectionViewModel(DriveBrowserViewModel browseVM)
    {
        RefreshAsyncLock = new AsyncLock();

        BrowseVM = browseVM;
        Setup();
        Drives = new ObservableCollection<DriveViewModel>();
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Properties

    private AsyncLock RefreshAsyncLock { get; }

    #endregion

    #region Public Properties

    /// <summary>
    /// The browse view model
    /// </summary>
    public DriveBrowserViewModel BrowseVM { get; }

    /// <summary>
    /// The currently available drives
    /// </summary>
    public ObservableCollection<DriveViewModel> Drives { get; }

    /// <summary>
    /// The current result
    /// </summary>
    public DriveBrowserResult Result { get; set; }

    /// <summary>
    /// The currently selected item
    /// </summary>
    public DriveViewModel SelectedItem { get; set; }

    /// <summary>
    /// The currently selected items
    /// </summary>
    public IList SelectedItems { get; set; }

    #endregion

    #region Private Methods

    /// <summary>
    /// Sets up the view model
    /// </summary>
    private void Setup()
    {
        Result = new DriveBrowserResult()
        {
            CanceledByUser = true
        };
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Updates the return value
    /// </summary>
    public void UpdateReturnValue()
    {
        Result.SelectedDrive = SelectedItem?.Path ?? FileSystemPath.EmptyPath;
        Result.SelectedDrives = SelectedItems?.Cast<DriveViewModel>().Select(x => x?.Path ?? FileSystemPath.EmptyPath) ?? new FileSystemPath[]{};
    }

    /// <summary>
    /// Refreshes the available drives
    /// </summary>
    public async Task RefreshAsync()
    {
        using (await RefreshAsyncLock.LockAsync())
        {
            Drives.Clear();

            DriveInfo[] drives;

            try
            {
                drives = DriveInfo.GetDrives();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Getting drives");
                    
                await Services.MessageUI.DisplayMessageAsync(Resources.DriveSelection_RefreshError, MessageType.Error);
                    
                return;
            }

            foreach (var drive in drives)
            {
                if (BrowseVM.AllowedTypes != null && !BrowseVM.AllowedTypes.Contains(drive.DriveType))
                    continue;

                ImageSource icon = null;
                string label = null;
                string path;
                string format = null;
                long? freeSpace = null;
                long? totalSize = null;
                DriveType? type = null;
                bool? ready = null;

                try
                {
                    label = drive.VolumeLabel;
                }
                catch (Exception ex)
                {
                    Logger.Debug(ex, "Getting drive label");
                }

                try
                {
                    path = drive.Name;

                    try
                    {
                        using var shellObj = ShellObject.FromParsingName(path);
                        var thumb = shellObj.Thumbnail;
                        thumb.CurrentSize = new System.Windows.Size(16, 16);
                        icon = thumb.GetTransparentBitmap()?.ToImageSource();
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug(ex, "Getting drive icon");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Debug(ex, "Getting drive name");
                    continue;
                }

                try
                {
                    format = drive.DriveFormat;
                }
                catch (Exception ex)
                {
                    Logger.Debug(ex, "Getting drive format");
                }

                try
                {
                    freeSpace = drive.TotalFreeSpace;
                }
                catch (Exception ex)
                {
                    Logger.Debug(ex, "Getting drive freeSpace");
                }

                try
                {
                    totalSize = drive.TotalSize;
                }
                catch (Exception ex)
                {
                    Logger.Debug(ex, "Getting drive totalSize");
                }

                try
                {
                    type = drive.DriveType;
                }
                catch (Exception ex)
                {
                    Logger.Debug(ex, "Getting drive type");
                }

                try
                {
                    ready = drive.IsReady;
                    if (!drive.IsReady && !BrowseVM.AllowNonReadyDrives)
                        continue;
                }
                catch (Exception ex)
                {
                    Logger.Debug(ex, "Getting drive ready");
                    if (!BrowseVM.AllowNonReadyDrives)
                        continue;
                }

                // Create the view model
                var vm = new DriveViewModel()
                {
                    Path = path,
                    Icon = icon,
                    Format = format,
                    Label = label,
                    Type = type,
                    FreeSpace = freeSpace,
                    TotalSize = totalSize,
                    IsReady = ready
                };

                Drives.Add(vm);
            }
        }
    }

    #endregion

    #region Commands

    private ICommand _RefreshCommand;

    /// <summary>
    /// A command for <see cref="RefreshAsync"/>
    /// </summary>
    public ICommand RefreshCommand => _RefreshCommand ??= new AsyncRelayCommand(RefreshAsync);

    #endregion
}