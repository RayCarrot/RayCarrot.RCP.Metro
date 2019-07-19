using System.Windows.Input;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A Rayman 2 key item view model
    /// </summary>
    public class R2KeyItemViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="actionName">The name of the action the key represents</param>
        /// <param name="originalKey">The original key for the action</param>
        /// <param name="parent">The parent view model</param>
        public R2KeyItemViewModel(string actionName, Key originalKey, Rayman2ConfigViewModel parent)
        {
            ActionName = actionName;
            OriginalKey = _newKey = originalKey;
            Parent = parent;

            ResetCommand = new RelayCommand(Reset);
        }

        #endregion

        #region Private Fields

        private Key _newKey;

        #endregion

        #region Private Properties

        /// <summary>
        /// The parent view model
        /// </summary>
        private Rayman2ConfigViewModel Parent { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The name of the action the key represents
        /// </summary>
        public string ActionName { get; }

        /// <summary>
        /// The original key for the action
        /// </summary>
        public Key OriginalKey { get; }

        /// <summary>
        /// The new key for the action
        /// </summary>
        public Key NewKey
        {
            get => _newKey;
            set
            {
                _newKey = value;
                Parent.UnsavedChanges = true;
            }
        }

        #endregion

        #region Commands

        public RelayCommand ResetCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resets the new key to the original key
        /// </summary>
        public void Reset()
        {
            NewKey = OriginalKey;
        }

        /// <summary>
        /// Sets the initial value for the new key
        /// </summary>
        /// <param name="newKey">The new key for the action</param>
        public void SetInitialNewKey(Key newKey)
        {
            _newKey = newKey;
            OnPropertyChanged(nameof(NewKey));
        }

        #endregion
    }
}