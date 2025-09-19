#nullable disable
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A binding proxy to use for binding to a stored data context
/// </summary>
public class BindingProxy : Freezable
{
    protected override Freezable CreateInstanceCore()
    {
        return new BindingProxy();
    }

    /// <summary>
    /// The data context to bind to
    /// </summary>
    public object Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public static readonly DependencyProperty DataProperty = DependencyProperty.Register(nameof(Data), typeof(object), typeof(BindingProxy));
}