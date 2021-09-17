using System;
using System.ComponentModel;
using MahApps.Metro.Controls;

namespace RayCarrot.RCP.Metro
{
    public class StandardWindowInstance : WindowInstance
    {
        public StandardWindowInstance(MetroWindow window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));

            Window.Closing += Window_Closing;
            Window.Closed += Window_Closed;
        }

        private MetroWindow _window;
        public MetroWindow Window => _window ?? throw new InvalidOperationException("The window instance can't be accessed after the window has been closed");

        public override string Title
        {
            get => Window.Title;
            set => Window.Title = value;
        }

        public override bool CanClose
        {
            get => Window.IsCloseButtonEnabled;
            set => Window.IsCloseButtonEnabled = value;
        }

        public override void Close() => Window.Close();
        public override void Focus() => Window.Focus();

        private void Window_Closing(object sender, CancelEventArgs e) => WindowClosing?.Invoke(sender, e);
        private void Window_Closed(object sender, EventArgs e)
        {
            WindowClosed?.Invoke(sender, e);

            Window.Closing -= Window_Closing;
            Window.Closed -= Window_Closed;
            _window = null;
        }

        public override event EventHandler<CancelEventArgs> WindowClosing;
        public override event EventHandler WindowClosed;
    }
}