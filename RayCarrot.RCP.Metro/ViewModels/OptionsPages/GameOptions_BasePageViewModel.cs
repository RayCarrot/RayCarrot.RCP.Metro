using System;
using System.Threading.Tasks;
using MahApps.Metro.IconPacks;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    public abstract class GameOptions_BasePageViewModel : BaseRCPViewModel, IDisposable
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="pageContent">The page UI content</param>
        /// <param name="pageName">The name of the page</param>
        /// <param name="pageIcon">The page icon</param>
        protected GameOptions_BasePageViewModel(LocalizedString pageName, PackIconMaterialKind pageIcon)
        {
            PageName = pageName;
            PageIcon = pageIcon;
        }

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

        #endregion

        #region Protected Methods

        protected abstract object GetPageUI();

        protected virtual Task LoadAsync() => Task.CompletedTask;
        //protected virtual Task SaveAsync() => Task.CompletedTask;

        protected void OnSaved() => Saved?.Invoke(this, EventArgs.Empty);

        #endregion

        #region Public Methods

        public Task LoadPageAsync()
        {
            // Create the page if it doesn't exist
            if (PageContent == null)
                PageContent = GetPageUI();

            // Load the page
            return Task.Run(async () => await LoadAsync());
        }

        //public Task SavePageAsync()
        //{
        //    // Save the page
        //    return LoadAsync();
        //}

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