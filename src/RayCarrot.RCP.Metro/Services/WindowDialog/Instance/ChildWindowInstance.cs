using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    public class ChildWindowInstance : WindowInstance
    {
        public ChildWindowInstance(RCPChildWindow window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));

            Window.Closing += Window_Closing;
            Window.IsOpenChanged += Window_IsOpenChanged;

            OpenChildWindows.Add(window);
        }

        public static List<RCPChildWindow> OpenChildWindows { get; } = new List<RCPChildWindow>();

        private RCPChildWindow _window;
        public RCPChildWindow Window => _window ?? throw new InvalidOperationException("The window instance can't be accessed after the window has been closed");

        public override string Title
        {
            get => Window.Title;
            set => Window.Title = value;
        }
        public override bool CanClose
        {
            get => Window.ShowCloseButton;
            set => Window.ShowCloseButton = value;
        }

        public override double Width
        {
            get => Window.ChildWindowWidth;
            set => Window.ChildWindowWidth = value;
        }
        public override double Height
        {
            get => Window.ChildWindowHeight;
            set => Window.ChildWindowHeight = value;
        }
        public override double MinWidth
        {
            get => Window.MinContentWidth;
            set => Window.MinContentWidth = value;
        }
        public override double MinHeight
        {
            get => Window.MinContentHeight;
            set => Window.MinContentHeight = value;
        }

        public override void Close() => Window.Close();
        public override void Focus() => Window.BringToFront();

        private void Window_Closing(object sender, CancelEventArgs e) => WindowClosing?.Invoke(sender, e);
        private void Window_IsOpenChanged(object sender, RoutedEventArgs e)
        {
            // If the window was opened we don't do anything
            if (Window.IsOpen)
                return;
            
            WindowClosed?.Invoke(sender, e);

            OpenChildWindows.Remove(Window);
            Window.Closing -= Window_Closing;
            Window.IsOpenChanged -= Window_IsOpenChanged;
            _window = null;
        }

        public override event EventHandler<CancelEventArgs> WindowClosing;
        public override event EventHandler WindowClosed;
    }
}