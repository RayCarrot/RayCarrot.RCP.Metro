using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Data;
using RayCarrot.Extensions;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Base for a progression view model
    /// </summary>
    public abstract class BaseProgressionViewModel : BaseRCPViewModel, IDisposable
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game</param>
        protected BaseProgressionViewModel(Games game)
        {
            // Set properties
            Game = game;
            ProgressionSlots = new ObservableCollection<ProgressionSlotViewModel>();

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
        public ObservableCollection<ProgressionSlotViewModel> ProgressionSlots { get; }

        #endregion

        #region Public Abstract Properties

        /// <summary>
        /// Indicates if the progression data is available
        /// </summary>
        public abstract bool IsAvailable { get; }

        #endregion

        #region Public Abstract Methods

        /// <summary>
        /// Loads the current save data if available
        /// </summary>
        /// <returns>The task</returns>
        public abstract Task LoadDataAsync();

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