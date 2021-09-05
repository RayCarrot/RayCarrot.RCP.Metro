using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Data;
using RayCarrot.IO;
using RayCarrot.Logging;

namespace RayCarrot.RCP.Metro
{
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
            RL.Logger?.LogInformationSource($"Progression data for {Game} is being loaded...");

            // Run on a new thread
            await Task.Run(() =>
            {
                try
                {
                    // Dispose existing slot view models
                    ProgressionSlots.DisposeAll();

                    RL.Logger?.LogDebugSource($"Existing slots have been disposed");

                    // Clear the collection
                    ProgressionSlots.Clear();

                    // Load the data
                    LoadData();

                    RL.Logger?.LogInformationSource($"Slots have been loaded");

                    // Remove empty slots
                    ProgressionSlots.RemoveWhere(x => x == null);

                    RL.Logger?.LogDebugSource($"Empty slots have been removed");
                }
                catch (Exception ex)
                {
                    ex.HandleError($"Reading {Game} save data");
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
}