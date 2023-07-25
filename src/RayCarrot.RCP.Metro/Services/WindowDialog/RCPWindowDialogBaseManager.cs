using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using System.Windows;

namespace RayCarrot.RCP.Metro;

public class RCPWindowDialogBaseManager : WindowDialogBaseManager
{
    public RCPWindowDialogBaseManager(AppUserData data)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }

    private AppUserData Data { get; }

    protected void ConfigureChildWindow(RCPChildWindow window, IWindowControl windowContent, bool isModal)
    {
        // Place the content within a transitioning content control for a transition when the window is opened
        var content = new TransitioningContentControl()
        {
            FocusVisualStyle = null,
            IsTabStop = false,
            Transition = Data.UI_EnableAnimations ? TransitionType.Left : TransitionType.Normal,
            UseLayoutRounding = true,
            Content = windowContent.UIContent,
            RestartTransitionOnContentChange = true
        };

        // For some reason the transition doesn't start automatically here, so we manually reload it on load (might be because the Loaded event fires twice, probably once before it gets shown)
        void Content_Loaded(object s, RoutedEventArgs e) => content.ReloadTransition();
        void Content_Unloaded(object s, RoutedEventArgs e)
        {
            // Make sure to unsubscribe to the events
            content.Unloaded -= Content_Unloaded;
            content.Loaded -= Content_Loaded;
        }
        content.Loaded += Content_Loaded;
        content.Unloaded += Content_Unloaded;

        // Set window properties
        window.Content = content;
        window.IsModal = isModal;
        window.CanMaximize = windowContent.IsResizable;
    }

    protected override Task ShowAsync(IWindowControl windowContent, bool isModal, string? title)
    {
        // Show as a child window
        if (Data.UI_UseChildWindows && App.Current?.ChildWindowsParent is MetroWindow metroWindow)
        {
            // Create the child window
            RCPChildWindow childWin = new();

            // Configure the window
            ConfigureChildWindow(childWin, windowContent, isModal);

            // Set the window instance
            windowContent.WindowInstance = new ChildWindowInstance(childWin);

            // Set the title
            if (title != null)
                windowContent.WindowInstance.Title = title;

            // Show the window
            return metroWindow.ShowChildWindowAsync(childWin);
        }
        // or show as a normal window
        else
        {
            return base.ShowAsync(windowContent, isModal, title);
        }
    }
}