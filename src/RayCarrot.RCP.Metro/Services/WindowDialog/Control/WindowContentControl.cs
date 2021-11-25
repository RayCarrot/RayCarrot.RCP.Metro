#nullable disable
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RayCarrot.RCP.Metro;

public abstract class WindowContentControl : UserControl, IWindowControl
{
    private WindowInstance _windowInstance;
    private bool _isClosing;
    private bool _forceClose;

    public object UIContent => this;
    public virtual bool IsResizable => false;
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

    private async void WindowInstance_WindowClosing(object sender, CancelEventArgs e)
    {
        // If we're set to force close we return without canceling the closing
        if (_forceClose)
        {
            _forceClose = false;
            return;
        }

        // Cancel the closing
        e.Cancel = true;

        // Return if already closing
        if (_isClosing)
            return;

        _isClosing = true;

        try
        {
            // Check if the window can be closed
            var canClose = await ClosingAsync();

            // If we can close we flag to force close and then close the window again
            if (canClose)
            {
                // Yield to make sure the closing event finishes running. You normally can't close a window while it's closing.
                await Dispatcher.Yield();

                _forceClose = true;

                WindowInstance.Close();
            }
        }
        finally
        {
            _isClosing = false;
        }
    }
    private void WindowInstance_WindowClosed(object sender, EventArgs e)
    {
        Closed();
    }

    protected virtual void WindowAttached() { }
    protected virtual Task<bool> ClosingAsync() => Task.FromResult(true);
    protected virtual void Closed()
    {
        _windowInstance.WindowClosing -= WindowInstance_WindowClosing;
        _windowInstance.WindowClosed -= WindowInstance_WindowClosed;
    }

    public virtual void Dispose() { }
}