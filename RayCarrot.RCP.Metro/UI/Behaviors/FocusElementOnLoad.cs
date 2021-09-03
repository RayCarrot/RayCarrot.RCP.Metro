using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Behavior for focusing the <see cref="FrameworkElement"/> on load
    /// </summary>
    public class FocusElementOnLoad : Behavior<FrameworkElement>
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
            AssociatedObject?.Focus();
        }
    }
}