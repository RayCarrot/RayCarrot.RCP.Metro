using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using static RayCarrot.RCP.Metro.GameIcon;

namespace RayCarrot.RCP.Metro;

public class GameTitle : Control, IRecipient<ModifiedGamesMessage>
{
    #region Constructor

    public GameTitle()
    {
        Loaded += GameTitle_OnLoaded;
        Unloaded += GameTitle_OnUnloaded;
    }

    #endregion

    #region GameInstallation

    public GameInstallation? GameInstallation
    {
        get => (GameInstallation?)GetValue(GameInstallationProperty);
        set => SetValue(GameInstallationProperty, value);
    }

    public static readonly DependencyProperty GameInstallationProperty = DependencyProperty.Register(
        nameof(GameInstallation), typeof(GameInstallation), typeof(GameTitle), new PropertyMetadata(OnGameInstallationChanged));

    private static void OnGameInstallationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        GameTitle gameTitle = (GameTitle)d;
        GameInstallation? gameInstallation = (GameInstallation?)e.NewValue;

        // If in design mode then App.Current is null and some things won't work. So we manually set some dummy data.
        if (DesignerProperties.GetIsInDesignMode(gameTitle))
        {
            gameTitle.GameIcon = GameIconAsset.Rayman2;
            gameTitle.IsDemo = false;
            gameTitle.GameDisplayName = "Rayman 2";
            gameTitle.PlatformIcon = GamePlatformIconAsset.Win32;
            gameTitle.PlatformDisplayName = Metro.Resources.Platform_Win32;
        }
        else if (gameInstallation != null)
        {
            GameDescriptor gameDescriptor = gameInstallation.GameDescriptor;
            GamePlatformInfoAttribute platformInfo = gameDescriptor.Platform.GetInfo();

            gameTitle.GameIcon = gameDescriptor.Icon;
            gameTitle.IsDemo = gameDescriptor.IsDemo;
            gameTitle.GameDisplayName = gameInstallation.GetDisplayName();
            gameTitle.PlatformIcon = platformInfo.Icon;
            gameTitle.PlatformDisplayName = platformInfo.DisplayName;
        }
        else
        {
            gameTitle.GameIcon = null;
            gameTitle.IsDemo = false;
            gameTitle.GameDisplayName = null;
            gameTitle.PlatformIcon = null;
            gameTitle.PlatformDisplayName = null;
        }
    }

    #endregion

    #region GameDescriptor

    public GameDescriptor? GameDescriptor
    {
        get => (GameDescriptor?)GetValue(GameDescriptorProperty);
        set => SetValue(GameDescriptorProperty, value);
    }

    public static readonly DependencyProperty GameDescriptorProperty = DependencyProperty.Register(
        nameof(GameDescriptor), typeof(GameDescriptor), typeof(GameTitle), new PropertyMetadata(OnGameDescriptorChanged));

    private static void OnGameDescriptorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        GameTitle gameTitle = (GameTitle)d;
        GameDescriptor? gameDescriptor = (GameDescriptor?)e.NewValue;

        // If in design mode then App.Current is null and some things won't work. So we manually set some dummy data.
        if (DesignerProperties.GetIsInDesignMode(gameTitle))
        {
            gameTitle.GameIcon = GameIconAsset.Rayman2;
            gameTitle.IsDemo = false;
            gameTitle.GameDisplayName = "Rayman 2";
            gameTitle.PlatformIcon = GamePlatformIconAsset.Win32;
            gameTitle.PlatformDisplayName = Metro.Resources.Platform_Win32;
        }
        else if (gameDescriptor != null)
        {
            GamePlatformInfoAttribute platformInfo = gameDescriptor.Platform.GetInfo();

            gameTitle.GameIcon = gameDescriptor.Icon;
            gameTitle.IsDemo = gameDescriptor.IsDemo;
            gameTitle.GameDisplayName = gameDescriptor.DisplayName;
            gameTitle.PlatformIcon = platformInfo.Icon;
            gameTitle.PlatformDisplayName = platformInfo.DisplayName;
        }
        else
        {
            gameTitle.GameIcon = null;
            gameTitle.IsDemo = false;
            gameTitle.GameDisplayName = null;
            gameTitle.PlatformIcon = null;
            gameTitle.PlatformDisplayName = null;
        }
    }

    #endregion

    #region GameIconSize

    public GameIconSize GameIconSize
    {
        get => (GameIconSize)GetValue(GameIconSizeProperty);
        set => SetValue(GameIconSizeProperty, value);
    }

    public static readonly DependencyProperty GameIconSizeProperty = DependencyProperty.Register(
        nameof(GameIconSize), typeof(GameIconSize), typeof(GameTitle), new PropertyMetadata(GameIconSize.Small));

    #endregion

    #region GameIcon

    public GameIconAsset? GameIcon
    {
        get => (GameIconAsset?)GetValue(GameIconProperty);
        private set => SetValue(GameIconPropertyKey, value);
    }

    private static readonly DependencyPropertyKey GameIconPropertyKey = 
        DependencyProperty.RegisterReadOnly(nameof(GameIcon), typeof(GameIconAsset?), typeof(GameTitle), new FrameworkPropertyMetadata());

    public static readonly DependencyProperty GameIconProperty = GameIconPropertyKey.DependencyProperty;

    #endregion

    #region IsDemo

    public bool IsDemo
    {
        get => (bool)GetValue(IsDemoProperty);
        private set => SetValue(IsDemoPropertyKey, value);
    }

    private static readonly DependencyPropertyKey IsDemoPropertyKey = 
        DependencyProperty.RegisterReadOnly(nameof(IsDemo), typeof(bool), typeof(GameTitle), new FrameworkPropertyMetadata());

    public static readonly DependencyProperty IsDemoProperty = IsDemoPropertyKey.DependencyProperty;

    #endregion

    #region GameDisplayName

    public LocalizedString? GameDisplayName
    {
        get => (LocalizedString?)GetValue(GameDisplayNameProperty);
        private set => SetValue(GameDisplayNamePropertyKey, value);
    }

    private static readonly DependencyPropertyKey GameDisplayNamePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(GameDisplayName), typeof(LocalizedString), typeof(GameTitle), new FrameworkPropertyMetadata());

    public static readonly DependencyProperty GameDisplayNameProperty = GameDisplayNamePropertyKey.DependencyProperty;

    #endregion

    #region PlatformIcon

    public GamePlatformIconAsset? PlatformIcon
    {
        get => (GamePlatformIconAsset?)GetValue(PlatformIconProperty);
        private set => SetValue(PlatformIconPropertyKey, value);
    }

    private static readonly DependencyPropertyKey PlatformIconPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(PlatformIcon), typeof(GamePlatformIconAsset?), typeof(GameTitle), new FrameworkPropertyMetadata());

    public static readonly DependencyProperty PlatformIconProperty = PlatformIconPropertyKey.DependencyProperty;

    #endregion

    #region PlatformDisplayName

    public LocalizedString? PlatformDisplayName
    {
        get => (LocalizedString?)GetValue(PlatformDisplayNameProperty);
        private set => SetValue(PlatformDisplayNamePropertyKey, value);
    }

    private static readonly DependencyPropertyKey PlatformDisplayNamePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(PlatformDisplayName), typeof(LocalizedString), typeof(GameTitle), new FrameworkPropertyMetadata());

    public static readonly DependencyProperty PlatformDisplayNameProperty = PlatformDisplayNamePropertyKey.DependencyProperty;

    #endregion

    #region Event Handlers

    private void GameTitle_OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (DesignerProperties.GetIsInDesignMode(this))
            return;

        Services.Messenger.UnregisterAll(this);
    }

    private void GameTitle_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DesignerProperties.GetIsInDesignMode(this))
            return;

        if (Services.Messenger.IsRegistered<ModifiedGamesMessage>(this))
            return;

        Services.Messenger.RegisterAll(this);
    }

    #endregion

    #region Message Receivers

    void IRecipient<ModifiedGamesMessage>.Receive(ModifiedGamesMessage message)
    {
        GameInstallation? gameInstallation = GameInstallation;

        if (gameInstallation != null)
            GameDisplayName = gameInstallation.GetDisplayName();
    }

    #endregion
}