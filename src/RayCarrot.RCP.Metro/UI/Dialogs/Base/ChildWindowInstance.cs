using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using MahApps.Metro.SimpleChildWindow;

namespace RayCarrot.RCP.Metro
{
    public class ChildWindowInstance : WindowInstance
    {
        public ChildWindowInstance(ChildWindow window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));

            Window.Closing += Window_Closing;
            Window.IsOpenChanged += Window_IsOpenChanged;

            OpenChildWindows.Add(window);
        }

        public static List<ChildWindow> OpenChildWindows { get; } = new List<ChildWindow>();

        private ChildWindow _window;
        public ChildWindow Window => _window ?? throw new InvalidOperationException("The window instance can't be accessed after the window has been closed");

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

        public override void Close() => Window.Close();
        public override void Focus() => Window.Focus();

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