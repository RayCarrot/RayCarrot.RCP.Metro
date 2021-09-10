using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Behavior for setting a unified margin in the items of a <see cref="Panel"/>
    /// </summary>
    public class UnifiedMarginBehavior : Behavior<Panel>
    {
        #region Public Properties

        /// <summary>
        /// The unified margin to use
        /// </summary>
        public Thickness Margin
        {
            get => (Thickness)GetValue(MarginProperty);
            set => SetValue(MarginProperty, value);
        }

        public static readonly DependencyProperty MarginProperty = DependencyProperty.Register(nameof(Margin), typeof(Thickness), typeof(UnifiedMarginBehavior), new FrameworkPropertyMetadata(new Thickness(0), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        #region Protected Overrides

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += Panel_Loaded;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= Panel_Loaded;
        }

        #endregion

        #region Event Handlers

        private void Panel_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the margin for all of the children
            foreach (UIElement child in AssociatedObject.Children)
            {
                if (child is not FrameworkElement fe || UnifiedMargin.GetIgnored(fe))
                    continue;

                // Set the margin
                fe.Margin = Margin;
            }
        }

        #endregion
    }
}