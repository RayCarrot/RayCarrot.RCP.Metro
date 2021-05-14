using MahApps.Metro.IconPacks;
using Nito.AsyncEx;
using RayCarrot.UI;
using RayCarrot.WPF;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    public abstract class GameOptions_BasePageViewModel : BaseRCPViewModel, IDisposable
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="pageName">The name of the page</param>
        /// <param name="pageIcon">The page icon</param>
        protected GameOptions_BasePageViewModel(LocalizedString pageName, PackIconMaterialKind pageIcon)
        {
            // Set properties
            PageName = pageName;
            PageIcon = pageIcon;
            AsyncLock = new AsyncLock();

            // Create commands
            SaveCommand = new AsyncRelayCommand(SavePageAsync);
            UseRecommendedCommand = new RelayCommand(UseRecommended);
        }

        #endregion

        #region Commands

        public ICommand SaveCommand { get; }
        public ICommand UseRecommendedCommand { get; }

        #endregion

        #region Private Properties

        /// <summary>
        /// The async lock to use for loading and saving
        /// </summary>
        private AsyncLock AsyncLock { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The page UI content
        /// </summary>
        public object PageContent { get; private set; }

        /// <summary>
        /// The name of the page
        /// </summary>
        public LocalizedString PageName { get; }

        /// <summary>
        /// The page icon
        /// </summary>
        public PackIconMaterialKind PageIcon { get; }

        /// <summary>
        /// Indicates if there are any unsaved changes
        /// </summary>
        public bool UnsavedChanges { get; set; }

        /// <summary>
        /// Indicates if the config should reload when the game info changes
        /// </summary>
        public virtual bool ReloadOnGameInfoChanged => false;

        /// <summary>
        /// Indicates if the page is fully loaded
        /// </summary>
        public bool IsLoaded { get; set; }

        /// <summary>
        /// Indicates if the page can be saved
        /// </summary>
        public virtual bool CanSave => false;

        /// <summary>
        /// Indicates if the option to use recommended options in the page is available
        /// </summary>
        public virtual bool CanUseRecommended => false;

        #endregion

        #region Protected Methods

        protected abstract object GetPageUI();

        protected virtual Task LoadAsync() => Task.CompletedTask;
        protected virtual Task<bool> SaveAsync() => Task.FromResult(false);
        protected virtual void UseRecommended() { }

        #endregion

        #region Public Methods

        public async Task LoadPageAsync()
        {
            using (await AsyncLock.LockAsync())
            {
                // Create the page if it doesn't exist
                if (PageContent == null)
                    PageContent = GetPageUI();

                // Load the page
                await Task.Run(LoadAsync);
            }
        }

        public async Task SavePageAsync()
        {
            using (await AsyncLock.LockAsync())
            {
                // Save the page
                var result = await SaveAsync();

                if (!result)
                    return;

                UnsavedChanges = false;

                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Config_SaveSuccess);

                Saved?.Invoke(this, EventArgs.Empty);
            }
        }

        public void SetErrorState(Exception ex)
        {
            PageContent = Services.Data.CurrentUserLevel >= UserLevel.Advanced ? ex.ToString() : null;
        }

        public virtual void Dispose()
        {
            PageName?.Dispose();
            PageContent = null;
            UnsavedChanges = false;
        }

        #endregion

        #region Events

        public event EventHandler Saved;

        #endregion
    }
}