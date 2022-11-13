﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for ArchiveGamePanelControl.xaml
/// </summary>
public partial class ArchiveGamePanelControl : UserControl
{
    public ArchiveGamePanelControl()
    {
        InitializeComponent();
    }

    private void ArchiveGamePanelControl_OnLoaded(object sender, RoutedEventArgs e)
    {
        // Hacky fix for closing the popup when you scroll with the mouse scroll bar
        ScrollViewer? parentScrollViewer = this.TryFindParent<ScrollViewer>();
        if (parentScrollViewer != null)
            parentScrollViewer.ScrollChanged += (_, _) => FullPathsPopup.IsOpen = false;

        // Unsubscribe as the loaded event will trigger each time this tab is selected
        Loaded -= ArchiveGamePanelControl_OnLoaded;
    }

    private void TrimmedPathsItemsControl_OnMouseEnter(object sender, MouseEventArgs e)
    {
        // Hacky fix for not opening popup while the control is transitioning or
        // else the popup will open in the wrong place and stay there
        TransitioningContentControl? transitionControl = this.TryFindParent<TransitioningContentControl>();
        if (transitionControl is { IsTransitioning: true })
        {
            transitionControl.TransitionCompleted += onTransitionCompleted;

            void onTransitionCompleted(object _sender, RoutedEventArgs _e)
            {
                FullPathsPopup.IsOpen = true;
                transitionControl.TransitionCompleted -= onTransitionCompleted;
            }

            return;
        }

        // Open on mouse over only if list is trimmed
        if (DataContext is ArchiveGamePanelViewModel { IsTrimmed: true })
            FullPathsPopup.IsOpen = true;
    }

    private void TrimmedPathsItemsControl_OnMouseLeave(object sender, MouseEventArgs e)
    {
        // Fix for an issue where the popup stays open if you move your mouse over it too quickly when opening
        if (!PopupCard.IsMouseOver)
            FullPathsPopup.IsOpen = false;
    }

    private void PopupCard_OnMouseLeave(object sender, MouseEventArgs e)
    {
        FullPathsPopup.IsOpen = false;
    }
}