using MahApps.Metro.SimpleChildWindow;
using RayCarrot.UI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A <see cref="ChildWindow"/> with support for minimizing and maximizing
    /// </summary>
    public class RCPChildWindow : ChildWindow
    {
        #region Constructor

        public RCPChildWindow()
        {
            ToggleMinimizedCommand = new RelayCommand(ToggleMinimized);
            ToggleMaximizedCommand = new RelayCommand(ToggleMaximized);

            ClosingFinished += RCPChildWindow_ClosingFinished;
        }

        #endregion

        #region Other

        public void BringToFront()
        {
            var container = Parent as Panel;
            var elementOnTop = container?.Children.OfType<UIElement>().OrderBy(c => c.GetValue(Panel.ZIndexProperty)).LastOrDefault();

            if (elementOnTop == null || Equals(elementOnTop, this)) 
                return;
            
            var zIndexOnTop = (int)elementOnTop.GetValue(Panel.ZIndexProperty);
            elementOnTop.SetCurrentValue(Panel.ZIndexProperty, zIndexOnTop - 1);
            SetCurrentValue(Panel.ZIndexProperty, zIndexOnTop);
        }

        #endregion

        #region Size

        public double MinContentWidth { get; set; }
        public double MinContentHeight { get; set; }

        private ContentPresenter _partContent;

        public double ActualContentWidth => _partContent?.ActualWidth ?? Double.NaN;
        public double ActualContentHeight => _partContent?.ActualHeight ?? Double.NaN;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_partContent != null)
                _partContent.SizeChanged -= PartContent_SizeChanged;

            _partContent = Template.FindName("PART_Content", this) as ContentPresenter;

            if (_partContent != null)
                _partContent.SizeChanged += PartContent_SizeChanged;
        }

        private bool _firstContentResize = true;

        private void PartContent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // We want to update the minimum window size whenever the content size changes
            App.Current?.ChildWindowsParent?.UpdateMinSize(true, true);

            // If it's the first resize we also center it afterwards as the window resizing might have changed it
            if (_firstContentResize)
            {
                _firstContentResize = false;
                OffsetX = 0;
                OffsetY = 0;
            }
        }

        private void RCPChildWindow_ClosingFinished(object sender, RoutedEventArgs e)
        {
            App.Current?.ChildWindowsParent?.UpdateMinSize(true, true);
        }

        #endregion

        #region Minimize

        public static ObservableCollection<MinimizedChildWindow> MinimizedWindows { get; } = new ObservableCollection<MinimizedChildWindow>();

        public ICommand ToggleMinimizedCommand { get; }

        public bool IsMinimized
        {
            get => (bool)GetValue(IsMinimizedProperty);
            set => SetValue(IsMinimizedProperty, value);
        }

        public static readonly DependencyProperty IsMinimizedProperty = DependencyProperty.Register(
            nameof(IsMinimized), typeof(bool), typeof(RCPChildWindow), new PropertyMetadata(false, IsMinimizedChanged));

        private static void IsMinimizedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                MinimizedWindows.Add(new MinimizedChildWindow((RCPChildWindow)d));
            else
                MinimizedWindows.RemoveWhere(x => x.Window == (RCPChildWindow)d);
        }

        public void ToggleMinimized()
        {
            IsMinimized = !IsMinimized;
        }

        // We need to wrap the windows in a class since we can't bind directly to the window due to how the logical tree works
        public record MinimizedChildWindow(RCPChildWindow Window);

        #endregion

        #region Maximize

        public ICommand ToggleMaximizedCommand { get; }

        public bool CanMaximize
        {
            get => (bool)GetValue(CanMaximizeProperty);
            set => SetValue(CanMaximizeProperty, value);
        }

        public static readonly DependencyProperty CanMaximizeProperty = DependencyProperty.Register(
            nameof(CanMaximize), typeof(bool), typeof(RCPChildWindow), new PropertyMetadata(false));

        public bool IsMaximized
        {
            get => (bool)GetValue(IsMaximizedProperty);
            set => SetValue(IsMaximizedProperty, value);
        }

        public static readonly DependencyProperty IsMaximizedProperty = DependencyProperty.Register(
            nameof(IsMaximized), typeof(bool), typeof(RCPChildWindow), new PropertyMetadata(false, IsMaximizedChanged));

        private double _normalWidth;
        private double _normalHeight;
        private double _normalOffsetX;
        private double _normalOffsetY;
        
        private static void IsMaximizedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var win = (RCPChildWindow)d;
            var isMaximized = (bool)e.NewValue;

            if (isMaximized)
            {
                win.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                win.VerticalContentAlignment = VerticalAlignment.Stretch;

                win._normalWidth = win.ChildWindowWidth;
                win._normalHeight = win.ChildWindowHeight;
                win.ChildWindowWidth = Double.NaN;
                win.ChildWindowHeight = Double.NaN;

                win._normalOffsetX = win.OffsetX;
                win._normalOffsetY = win.OffsetY;
                win.OffsetX = 0;
                win.OffsetY = 0;
            }
            else
            {
                win.HorizontalContentAlignment = HorizontalAlignment.Center;
                win.VerticalContentAlignment = VerticalAlignment.Center;

                win.ChildWindowWidth = win._normalWidth;
                win.ChildWindowHeight = win._normalHeight;

                win.OffsetX = win._normalOffsetX;
                win.OffsetY = win._normalOffsetY;
            }
        }

        public void ToggleMaximized()
        {
            IsMaximized = !IsMaximized;
        }

        #endregion
    }
}