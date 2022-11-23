﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using NLog;

namespace RayCarrot.RCP.Metro;

public class GameViewModel : BaseViewModel, IRecipient<RemovedGamesMessage>
{
    #region Constructor

    public GameViewModel(GameDescriptor gameDescriptor)
    {
        GameDescriptor = gameDescriptor;
        DisplayName = gameDescriptor.DisplayName;
        AddActions = new ObservableCollection<GameAddActionViewModel>(gameDescriptor.GetAddActions().
            // Reverse the order so the common actions are aligned in the ui
            Reverse().Select(x => new GameAddActionViewModel(x)));

        // Get and set platform info
        GamePlatformInfoAttribute platformInfo = gameDescriptor.Platform.GetInfo();
        PlatformDisplayName = platformInfo.DisplayName;
        PlatformIconSource = $"{AppViewModel.WPFApplicationBasePath}Img/GamePlatformIcons/{platformInfo.Icon.GetAttribute<ImageFileAttribute>()!.FileName}";

        AddGameCommand = new AsyncRelayCommand(x => AddGameAsync(((GameAddActionViewModel)x!).AddAction));

        Services.Messenger.RegisterAll(this);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand AddGameCommand { get; }

    #endregion

    #region Public Properties

    public GameDescriptor GameDescriptor { get; }
    public LocalizedString DisplayName { get; }
    public ObservableCollection<GameAddActionViewModel> AddActions { get; }

    public LocalizedString PlatformDisplayName { get; }
    public string PlatformIconSource { get; }

    #endregion

    #region Private Methods

    private void Refresh()
    {
        // Re-evaluate if each action is available
        foreach (GameAddActionViewModel addAction in AddActions)
            addAction.OnPropertyChanged(nameof(GameAddActionViewModel.IsAvailable));
    }

    #endregion

    #region Public Methods

    public async Task AddGameAsync(GameAddAction addAction)
    {
        try
        {
            // Call the add action
            GameInstallation? gameInstallation = await addAction.AddGameAsync();

            // Return if it failed
            if (gameInstallation == null)
                return;

            // Refresh
            Refresh();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Adding game from add action");
            // TODO-UPDATE: Rewrite this?
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.LocateGame_Error, Resources.LocateGame_ErrorHeader);
        }
    }

    public void Receive(RemovedGamesMessage message) => Refresh();

    #endregion

    #region Classes

    public class GameAddActionViewModel : BaseViewModel
    {
        public GameAddActionViewModel(GameAddAction addAction)
        {
            AddAction = addAction;
        }

        public GameAddAction AddAction { get; }

        public LocalizedString Header => AddAction.Header;
        public GenericIconKind Icon => AddAction.Icon;
        public bool IsAvailable => AddAction.IsAvailable;
    }

    #endregion
}