using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for OverflowButton.xaml
    /// </summary>
    public partial class OverflowButton : UserControl
    {
        #region Constructor

        public OverflowButton()
        {
            InitializeComponent();

            // Set the data context for binding
            ContainerGrid.DataContext = this;

            // Add a preview mouse down outside captured element handler
            Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OutsideCapturedElementHandler);

            Loaded += (s, e) => ParentScrollViewer = this.FindParent<ScrollViewer>();
        }

        #endregion

        #region Private Properties

        private ScrollViewer ParentScrollViewer { get; set; }

        #endregion

        #region Dependency Properties

        public IEnumerable<OverflowButtonItemViewModel> OverflowItemSource  
        {
            get => (IEnumerable<OverflowButtonItemViewModel>)GetValue(OverflowItemSourceProperty);
            set => SetValue(OverflowItemSourceProperty, value);
        }

        public static readonly DependencyProperty OverflowItemSourceProperty = DependencyProperty.Register(nameof(OverflowItemSource), typeof(IEnumerable<OverflowButtonItemViewModel>), typeof(OverflowButton), new PropertyMetadata(new OverflowButtonItemViewModel[0]));

        public object MainContent
        {
            get => GetValue(MainContentProperty);
            set => SetValue(MainContentProperty, value);
        }

        public static readonly DependencyProperty MainContentProperty = DependencyProperty.Register(nameof(MainContent), typeof(object), typeof(OverflowButton));

        public PackIconMaterialKind MainIconKind
        {
            get => (PackIconMaterialKind)GetValue(MainIconKindProperty);
            set => SetValue(MainIconKindProperty, value);
        }

        public static readonly DependencyProperty MainIconKindProperty = DependencyProperty.Register(nameof(MainIconKind), typeof(PackIconMaterialKind), typeof(OverflowButton));

        public ICommand MainCommand
        {
            get => (ICommand)GetValue(MainCommandProperty);
            set => SetValue(MainCommandProperty, value);
        }

        public static readonly DependencyProperty MainCommandProperty = DependencyProperty.Register(nameof(MainCommand), typeof(ICommand), typeof(OverflowButton));

        public bool ShowSecondaryButton
        {
            get => (bool)GetValue(ShowSecondaryButtonProperty);
            set => SetValue(ShowSecondaryButtonProperty, value);
        }

        public static readonly DependencyProperty ShowSecondaryButtonProperty = DependencyProperty.Register(nameof(ShowSecondaryButton), typeof(bool), typeof(OverflowButton));

        public ICommand SecondaryCommand
        {
            get => (ICommand)GetValue(SecondaryCommandProperty);
            set => SetValue(SecondaryCommandProperty, value);
        }

        public static readonly DependencyProperty SecondaryCommandProperty = DependencyProperty.Register(nameof(SecondaryCommand), typeof(ICommand), typeof(OverflowButton));

        public PackIconMaterialKind SecondaryIconKind
        {
            get => (PackIconMaterialKind)GetValue(SecondaryIconKindProperty);
            set => SetValue(SecondaryIconKindProperty, value);
        }

        public static readonly DependencyProperty SecondaryIconKindProperty = DependencyProperty.Register(nameof(SecondaryIconKind), typeof(PackIconMaterialKind), typeof(OverflowButton));

        #endregion

        #region Event Handlers

        private void ExpanderButton_OnClick(object sender, RoutedEventArgs e)
        {
            // Change the state of the popup
            PopupControl.IsOpen ^= true;
        }

        private void PopupOpened(object sender, EventArgs e)
        {
            Mouse.Capture(this, CaptureMode.SubTree);

            // Mouse capture can be lost on 'this' when the user clicks on the scroll bar, which can cause
            // OutsideCapturedElementHandler to never be called. If we monitor the popup for lost mouse capture
            // (which the popup gains on mouse down of the scroll bar), then we can recapture the mouse at that point
            // to cause OutsideCapturedElementHandler to be called again.
            Mouse.AddLostMouseCaptureHandler(PopupControl, LostMouseCaptureHandler);

            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.Deactivated += MainWindow_Deactivated;

            if (ParentScrollViewer != null)
                ParentScrollViewer.ScrollChanged += Sv_ScrollChanged;
        }

        private void Sv_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            PopupControl.IsOpen = false;
        }

        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            PopupControl.IsOpen = false;
        }

        private void PopupClosed(object sender, EventArgs e)
        {
            ReleaseMouseCapture();
            Mouse.RemoveLostMouseCaptureHandler(PopupControl, LostMouseCaptureHandler);

            if (IsKeyboardFocusWithin)
                PopupControl.Focus();

            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.Deactivated -= MainWindow_Deactivated;

            if (ParentScrollViewer != null)
                ParentScrollViewer.ScrollChanged -= Sv_ScrollChanged;
        }

        private void LostMouseCaptureHandler(object sender, MouseEventArgs e)
        {
            // If the list is still expanded, recapture the SplitButton subtree
            // so that we still can know when the user has clicked outside of the popup.
            // This happens on scroll bar mouse up, so this doesn't disrupt the scroll bar functionality
            // at all.
            if (PopupControl.IsOpen)
                Mouse.Capture(this, CaptureMode.SubTree);
        }

        private void OutsideCapturedElementHandler(object sender, MouseButtonEventArgs e)
        {
            PopupControl.IsOpen = false;
        }

        private void MainButton_OnClick(object sender, RoutedEventArgs e)
        {
            // Close the popup
            PopupControl.IsOpen = false;
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            PopupControl.IsOpen = false;
        }

        #endregion
    }
}