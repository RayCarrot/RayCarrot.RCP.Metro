﻿#nullable disable
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for DriveSelectionDialog.xaml
/// </summary>
public partial class DriveSelectionDialog : WindowContentControl, IDialogWindowControl<DriveBrowserViewModel, DriveBrowserResult>
{
    #region Constructors

    /// <summary>
    /// Creates a new instance of <see cref="DriveSelectionDialog"/> with default values
    /// </summary>
    public DriveSelectionDialog()
    {
        InitializeComponent();

        ViewModel = new DriveBrowserViewModel()
        {
            Title = Metro.Resources.Browse_SelectDrive
        };
        DataContext = new DriveSelectionViewModel(ViewModel);
    }

    /// <summary>
    /// Creates a new instance of <see cref="DriveSelectionDialog"/> from a browse view model
    /// </summary>
    /// <param name="vm">The view model</param>
    public DriveSelectionDialog(DriveBrowserViewModel vm)
    {
        InitializeComponent();
        ViewModel = vm;
        DataContext = new DriveSelectionViewModel(ViewModel);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    /// <summary>
    /// The view model
    /// </summary>
    public DriveBrowserViewModel ViewModel { get; }

    /// <summary>
    /// The drive selection view model
    /// </summary>
    public DriveSelectionViewModel DriveSelectionVM => DataContext as DriveSelectionViewModel;

    #endregion

    #region Private Methods

    private async Task AttemptConfirmAsync()
    {
        DriveSelectionVM.UpdateReturnValue();

        // Make sure a drive was selected
        if (DriveSelectionVM.Result.SelectedDrives == null || !DriveSelectionVM.Result.SelectedDrives.Any())
        {
            Logger.Warn("No drive has been selected");
            return;
        }

        if (!DriveSelectionVM.Result.SelectedDrives.Select(x => new FileSystemPath(x)).DirectoriesExist())
        {
            Logger.Warn("Selected drive no longer exists");
            await DriveSelectionVM.RefreshAsync();
            return;
        }

        if (!DriveSelectionVM.BrowseVM.AllowNonReadyDrives && DriveSelectionVM.Result.SelectedDrives.Any(x =>
            {
                try
                {
                    return !new DriveInfo(x).IsReady;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Checking if drive is ready");
                    return true;
                }
            }))
        {
            Logger.Warn("One or more of the selected drives are not ready");
            await DriveSelectionVM.RefreshAsync();
            return;
        }

        DriveSelectionVM.Result.CanceledByUser = false;
        WindowInstance.Close();
    }

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Icon = GenericIconKind.Window_DriveSelection;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets the current result
    /// </summary>
    /// <returns>The result</returns>
    public DriveBrowserResult GetResult()
    {
        DriveSelectionVM.UpdateReturnValue();
        return DriveSelectionVM.Result;
    }

    #endregion

    #region Event Handlers

    private async void Continue_ClickAsync(object sender, RoutedEventArgs e)
    {
        await AttemptConfirmAsync();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DriveSelectionVM.Result.CanceledByUser = true;
        WindowInstance.Close();
    }

    private async void DriveItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        await AttemptConfirmAsync();
    }

    private async void UserControl_LoadedAsync(object sender, RoutedEventArgs e)
    {
        await DriveSelectionVM.RefreshAsync();
    }

    #endregion

}