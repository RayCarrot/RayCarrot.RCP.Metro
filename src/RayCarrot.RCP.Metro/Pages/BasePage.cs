#nullable disable
using System;
using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The base for a page control
/// </summary>
/// <typeparam name="VM">The view model type</typeparam>
public class BasePage<VM> : VMUserControl<VM>, IBasePage
    where VM : BaseViewModel, new()
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public BasePage()
    {
        Loaded += BasePage_Loaded;
    }

    /// <summary>
    /// Constructor for passing in a view model instance
    /// </summary>
    /// <param name="instance">The instance of the view model to use</param>
    public BasePage(VM instance) : base(instance)
    {
        Loaded += BasePage_Loaded;
    }

    /// <summary>
    /// The overflow menu
    /// </summary>
    public ContextMenu OverflowMenu { get; set; }

    private void BasePage_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= BasePage_Loaded;

        // Attempt to get parent window
        var window = Window.GetWindow(this);

        if (window == null)
            return;

        void Window_Closed(object ss, EventArgs ee)
        {
            window.Closed -= Window_Closed;
            (ViewModel as IDisposable)?.Dispose();
        }

        window.Closed += Window_Closed;
    }
}