#nullable disable
using System.ComponentModel;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// An abstract window instance
/// </summary>
public abstract class WindowInstance
{
    public abstract string Title { get; set; } // TODO: LocalizedString?
    public abstract bool CanClose { get; set; }

    public abstract GenericIconKind Icon { get; set; }

    public abstract double Width { get; set; }
    public abstract double Height { get; set; }
    public abstract double MinWidth { get; set; }
    public abstract double MinHeight { get; set; }

    public abstract void Close();
    public abstract void Focus();

    public abstract event EventHandler<CancelEventArgs> WindowClosing;
    public abstract event EventHandler WindowClosed;
}