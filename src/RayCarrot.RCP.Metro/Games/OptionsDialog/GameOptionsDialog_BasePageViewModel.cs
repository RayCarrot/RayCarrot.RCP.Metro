#nullable disable
using Nito.AsyncEx;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using NLog;

namespace RayCarrot.RCP.Metro;

public abstract class GameOptionsDialog_BasePageViewModel : BaseRCPViewModel, IDisposable, IRecipient<ModifiedGamesMessage>
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="pageName">The name of the page</param>
    /// <param name="pageIcon">The page icon</param>
    protected GameOptionsDialog_BasePageViewModel(LocalizedString pageName, GenericIconKind pageIcon)
    {
        // Set properties
        PageName = pageName;
        PageIcon = pageIcon;
        AsyncLock = new AsyncLock();

        // Create commands
        SaveCommand = new AsyncRelayCommand(SavePageAsync);
        UseRecommendedCommand = new RelayCommand(UseRecommended);
        PageSelectionIndexChangedCommand = new AsyncRelayCommand(PageSelectionIndexChangedAsync);

        // Register for messages
        Services.Messenger.RegisterAll(this);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand SaveCommand { get; }
    public ICommand UseRecommendedCommand { get; }
    public ICommand PageSelectionIndexChangedCommand { get; }

    #endregion

    #region Private Fields

    private int _selectedPageSelectionIndex;

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
    public GenericIconKind PageIcon { get; }

    /// <summary>
    /// Indicates if there are any unsaved changes
    /// </summary>
    public bool UnsavedChanges { get; set; }

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

    /// <summary>
    /// Optional selection for the page
    /// </summary>
    public virtual ObservableCollection<string> PageSelection => null;

    /// <summary>
    /// The selected index of <see cref="PageSelection"/>
    /// </summary>
    public int SelectedPageSelectionIndex
    {
        get => _selectedPageSelectionIndex;
        set
        {
            Logger.Debug("Changing page selection from {0} to {1}", _selectedPageSelectionIndex, value);
            _selectedPageSelectionIndex = value;
            PageSelectionIndexChangedCommand.Execute(null);
        }
    }

    #endregion

    #region Protected Methods

    protected abstract object GetPageUI();

    protected virtual Task LoadAsync() => Task.CompletedTask;
    protected virtual Task<bool> SaveAsync() => Task.FromResult(false);
    protected virtual Task OnGameInfoModifiedAsync() => Task.CompletedTask;
    protected virtual void UseRecommended() { }

    protected void ResetSelectedPageSelectionIndex()
    {
        Logger.Debug("Resetting page selection index to 0");
        _selectedPageSelectionIndex = 0;
        OnPropertyChanged(nameof(SelectedPageSelectionIndex));
    }

    #endregion

    #region Public Methods

    public async void Receive(ModifiedGamesMessage message)
    {
        // TODO-14: Check for same game? Otherwise this will be invoked for all games...
        await OnGameInfoModifiedAsync();
    }

    public virtual Task PageSelectionIndexChangedAsync() => Task.CompletedTask;

    public async Task LoadPageAsync()
    {
        using (await AsyncLock.LockAsync())
        {
            // Create the page if it doesn't exist
            PageContent ??= RCP.Metro.App.Current.Dispatcher.Invoke(GetPageUI);

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
        PageContent = Services.InstanceData.CurrentUserLevel >= UserLevel.Advanced ? ex.ToString() : null;
    }

    public virtual void Dispose()
    {
        PageName?.Dispose();
        PageContent = null;
        UnsavedChanges = false;

        // Unregister for messages
        Services.Messenger.UnregisterAll(this);
    }

    #endregion

    #region Events

    public event EventHandler Saved;

    #endregion
}