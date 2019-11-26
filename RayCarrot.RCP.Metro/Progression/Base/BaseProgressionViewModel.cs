using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using RayCarrot.CarrotFramework.Abstractions;
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
            Game = game;
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
        public ObservableCollection<ProgressionSlotViewModel> ProgressionSlots { get; set; }

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

        public void Dispose()
        {
            // Dispose each item
            ProgressionSlots?.ForEach(x => x.Dispose());
        }
    }

    /// <summary>
    /// View model for a progression slot item
    /// </summary>
    public class ProgressionSlotViewModel : BaseRCPViewModel, IDisposable
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="slotNameGenerator">The function to get the slot name</param>
        /// <param name="items">The progression info items</param>
        public ProgressionSlotViewModel(Func<string> slotNameGenerator, ProgressionInfoItemViewModel[] items)
        {
            SlotNameGenerator = slotNameGenerator;
            Items = items;
            SlotName = SlotNameGenerator();

            RCFCore.Data.CultureChanged += Data_CultureChanged;
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// The function to get the slot name
        /// </summary>
        protected Func<string> SlotNameGenerator { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The slot name
        /// </summary>
        public string SlotName { get; set; }

        /// <summary>
        /// The progression info items
        /// </summary>
        public ProgressionInfoItemViewModel[] Items { get; }

        #endregion

        #region Event Handlers

        private void Data_CultureChanged(object sender, PropertyChangedEventArgs<CultureInfo> e)
        {
            SlotName = SlotNameGenerator();
        }

        #endregion

        #region Public Methods

        public void Dispose()
        {
            RCFCore.Data.CultureChanged -= Data_CultureChanged;
        }

        #endregion
    }

    /// <summary>
    /// View model for a progression info item
    /// </summary>
    public class ProgressionInfoItemViewModel : BaseRCPViewModel
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="icon">The icon</param>
        /// <param name="content">The content</param>
        public ProgressionInfoItemViewModel(ProgressionIcons icon, string content)
        {
            Icon = icon;
            Content = content;
        }

        /// <summary>
        /// The icon
        /// </summary>
        public ProgressionIcons Icon { get; }

        /// <summary>
        /// The icon as an image source
        /// </summary>
        public ImageSource IconImageSource => new ImageSourceConverter().ConvertFrom($"{AppViewModel.ApplicationBasePath}Img/ProgressionIcons/UbiArt/{Icon}.png") as ImageSource;

        /// <summary>
        /// The content
        /// </summary>
        public string Content { get; }
    }

    /// <summary>
    /// The available progression icons
    /// </summary>
    public enum ProgressionIcons
    {
        Clock,
        Electoon,
        Lum,
        Medal, 
        RedTooth,
        Trophy
    }
}