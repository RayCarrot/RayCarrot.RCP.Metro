#nullable disable
using System;
using System.ComponentModel;

namespace RayCarrot.RCP.Metro;

public class StandardWindowInstance : WindowInstance
{
    public StandardWindowInstance(BaseIconWindow window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));

        Window.Closing += Window_Closing;
        Window.Closed += Window_Closed;
    }

    private BaseIconWindow _window;
    public BaseIconWindow Window => _window ?? throw new InvalidOperationException("The window instance can't be accessed after the window has been closed");

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

    public override GenericIconKind Icon
    {
        get => Window.GenericIcon;
        set => Window.GenericIcon = value;
    }

    public override double Width
    {
        get => Window.Width;
        set => Window.Width = value;
    }
    public override double Height
    {
        get => Window.Height;
        set => Window.Height = value;
    }
    public override double MinWidth
    {
        get => Window.MinWidth;
        set => Window.MinWidth = value;
    }
    public override double MinHeight
    {
        get => Window.MinHeight;
        set => Window.MinHeight = value;
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