#nullable disable
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Data;
using RayCarrot.IO;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Base for a progression view model
/// </summary>
public abstract class GameProgression_BaseViewModel : BaseRCPViewModel, IDisposable
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="game">The game</param>
    protected GameProgression_BaseViewModel(Games game)
    {
        // Set properties
        Game = game;
        ProgressionSlots = new ObservableCollection<GameProgression_BaseSlotViewModel>();

        // Enable collection synchronization
        BindingOperations.EnableCollectionSynchronization(ProgressionSlots, this);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    /// <summary>
    /// The game
    /// </summary>
    public Games Game { get; }

    /// <summary>
    /// The save data directory
    /// </summary>
    public FileSystemPath SaveDir { get; protected set; }

    /// <summary>
    /// The available progression slots. These should get set when loading the data.
    /// </summary>
    public ObservableCollection<GameProgression_BaseSlotViewModel> ProgressionSlots { get; }

    #endregion

    #region Protected Abstract Methods

    /// <summary>
    /// Loads the current save data if available
    /// </summary>
    protected abstract void LoadData();

    #endregion

    #region Public Methods

    /// <summary>
    /// Loads the current save data if available
    /// </summary>
    /// <returns>The task</returns>
    public async Task LoadDataAsync()
    {
        Logger.Info("Progression data for {0} is being loaded...", Game);

        // Run on a new thread
        await Task.Run(() =>
        {
            try
            {
                // Dispose existing slot view models
                ProgressionSlots.DisposeAll();

                Logger.Debug("Existing slots have been disposed");

                // Clear the collection
                ProgressionSlots.Clear();

                // Load the data
                LoadData();

                Logger.Info("Slots have been loaded");

                // Remove empty slots
                ProgressionSlots.RemoveWhere(x => x == null);

                Logger.Debug("Empty slots have been removed");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Reading {0} save data", Game);
                throw;
            }
        });
    }

    #endregion

    #region Public Methods

    public void Dispose()
    {
        // Dispose each item
        ProgressionSlots?.ForEach(x => x.Dispose());

        // Disable collection synchronization
        BindingOperations.DisableCollectionSynchronization(ProgressionSlots);
    }

    #endregion
}