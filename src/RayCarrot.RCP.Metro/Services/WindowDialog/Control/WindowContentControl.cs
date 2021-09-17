using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    public abstract class WindowContentControl : UserControl, IWindowControl
    {
        private WindowInstance _windowInstance;

        public object UIContent => this;
        public virtual IWindowControl.WindowResizeMode ResizeMode => IWindowControl.WindowResizeMode.NoResize;
        public WindowInstance WindowInstance
        {
            get => _windowInstance;
            set
            {
                if (_windowInstance != null)
                {
                    _windowInstance.WindowClosing -= WindowInstance_WindowClosing;
                    _windowInstance.WindowClosed -= WindowInstance_WindowClosed;
                }

                if (value != null)
                {
                    value.WindowClosing += WindowInstance_WindowClosing;
                    value.WindowClosed += WindowInstance_WindowClosed;
                }

                _windowInstance = value;

                if (value != null)
                    WindowAttached();
            }
        }

        private void WindowInstance_WindowClosing(object sender, CancelEventArgs e)
        {
            var canClose = Closing();

            if (!canClose)
                e.Cancel = true;
        }
        private void WindowInstance_WindowClosed(object sender, EventArgs e)
        {
            Closed();
        }

        protected virtual void WindowAttached() { }
        protected virtual bool Closing() => true;
        protected virtual void Closed()
        {
            _windowInstance.WindowClosing -= WindowInstance_WindowClosing;
            _windowInstance.WindowClosed -= WindowInstance_WindowClosed;
        }
    }
}