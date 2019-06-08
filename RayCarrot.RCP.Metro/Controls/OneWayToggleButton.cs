using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A one-way toggle button, where it can only be checked from the control itself
    /// </summary>
    public class OneWayToggleButton : Button
    {
        static OneWayToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OneWayToggleButton), new FrameworkPropertyMetadata(typeof(OneWayToggleButton)));
        }

        public OneWayToggleButton()
        {
            Click += OneWayToggleButton_Click;
        }

        private void OneWayToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsChecked)
                IsChecked = true;
        }

        /// <summary>
        /// Indicates if the button is checked or not
        /// </summary>
        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        /// <summary>
        /// Dependency property for <see cref="IsChecked"/>
        /// </summary>
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(OneWayToggleButton), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    }
}