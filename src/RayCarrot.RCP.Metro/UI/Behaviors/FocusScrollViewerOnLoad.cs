#nullable disable
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Behavior for focusing the parent <see cref="ScrollViewer"/> on load
/// </summary>
public class FocusScrollViewerOnLoad : Behavior<FrameworkElement>
{
    protected override void OnAttached()
    {
        AssociatedObject.Loaded += AssociatedObject_Loaded;       
    }

    protected override void OnDetaching()
    {
        AssociatedObject.Loaded -= AssociatedObject_Loaded;
    }

    private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
    {
        GetRootScrollViewer()?.Focus();
    }

    private ScrollViewer GetRootScrollViewer()
    {
        DependencyObject el = AssociatedObject;

        while (el != null && !(el is ScrollViewer))
            el = VisualTreeHelper.GetParent(el);

        return el as ScrollViewer;
    }
}