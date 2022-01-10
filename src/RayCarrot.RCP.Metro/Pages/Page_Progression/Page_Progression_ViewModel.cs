﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Nito.AsyncEx;
using NLog;

namespace RayCarrot.RCP.Metro;

public class Page_Progression_ViewModel : BasePageViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public Page_Progression_ViewModel()
    {
        // Create properties
        GameItems = new ObservableCollection<ProgressionGameViewModel>();
        AsyncLock = new AsyncLock();

        // Create commands
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        BackupAllCommand = new AsyncRelayCommand(BackupAllAsync);
        ChangeSaveEditProgramCommand = new AsyncRelayCommand(ChangeSaveEditProgramAsync);

        // Enable collection synchronization
        BindingOperations.EnableCollectionSynchronization(GameItems, Application.Current);

        // Refresh on app refresh
        App.RefreshRequired += async (_, e) =>
        {
            if (e.BackupsModified || e.GameCollectionModified)
                await Task.Run(async () => await RefreshAsync());
        };
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Properties

    private AsyncLock AsyncLock { get; }

    #endregion

    #region Commands

    public ICommand RefreshCommand { get; }
    public ICommand BackupAllCommand { get; }
    public ICommand ChangeSaveEditProgramCommand { get; }

    #endregion

    #region Public Properties

    public override AppPage Page => AppPage.Progression;
    public ObservableCollection<ProgressionGameViewModel> GameItems { get; }

    #endregion

    #region Public Methods

    protected override Task InitializeAsync() => RefreshAsync();

    public async Task RefreshAsync()
    {
        using (await AsyncLock.LockAsync())
        {
            try
            {
                Logger.Info("Refreshing progression game items");

                // Clear current items
                GameItems.Clear();

                // Add the game items
                foreach (Games game in App.GetGames.Where(x => x.IsAdded()))
                    GameItems.AddRange(game.GetGameInfo().GetProgressionGameViewModels);

                // Load the game items
                foreach (ProgressionGameViewModel game in GameItems)
                    await game.LoadProgressAsync();

                // Load backups
                foreach (ProgressionGameViewModel game in GameItems)
                    await game.LoadBackupAsync();

                // Load slot infos
                foreach (ProgressionGameViewModel game in GameItems)
                    await game.LoadSlotInfoItemsAsync();

                Logger.Info("Refreshed progression game items");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Refreshing progression");
                throw;
            }
        }
    }

    public async Task BackupAllAsync()
    {
        // Make sure no backups are running
        if (GameItems.Any(x => x.IsPerformingBackupRestore))
            return;

        // Lock
        using (await AsyncLock.LockAsync())
        {
            // Confirm backup
            if (!await Services.MessageUI.DisplayMessageAsync(Resources.Backup_ConfirmBackupAll, Resources.Backup_ConfirmBackupAllHeader, MessageType.Warning, true))
            {
                Logger.Info("Backup canceled");

                return;
            }

            int completed = 0;

            // Perform each backup
            foreach (ProgressionGameViewModel game in GameItems)
            {
                if (await game.BackupAsync(true))
                    completed++;
            }

            if (completed == GameItems.Count)
                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Backup_BackupAllSuccess);
            else
                await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Backup_BackupAllFailed, completed, GameItems.Count), Resources.Backup_BackupAllFailedHeader, MessageType.Information);
        }
    }

    public async Task ChangeSaveEditProgramAsync()
    {
        ProgramSelectionResult programResult = await Services.UI.GetProgramAsync(new ProgramSelectionViewModel()
        {
            Title = Resources.Progression_SelectEditProgram,
            ProgramFilePath = Data.Progression_SaveEditorExe,
            FileExtensions = new FileExtension[]
            {
                new FileExtension(".json"),
                new FileExtension(".txt"),
            }
        });

        if (programResult.CanceledByUser)
            return;

        Data.Progression_SaveEditorExe = programResult.ProgramFilePath;
    }

    #endregion
}