using System.Windows;
using RayCarrot.WPF;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A duo grid item
    /// </summary>
    public class DuoGridItem : Control
    {
        /// <summary>
        /// The header
        /// </summary>
        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(string), typeof(DuoGridItem));

        /// <summary>
        /// The text to display
        /// </summary>
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(DuoGridItem));

        /// <summary>
        /// The minimum user level for this item
        /// </summary>
        public UserLevel MinUserLevel
        {
            get => (UserLevel)GetValue(MinUserLevelProperty);
            set => SetValue(MinUserLevelProperty, value);
        }

        public static readonly DependencyProperty MinUserLevelProperty = DependencyProperty.Register(nameof(MinUserLevel), typeof(UserLevel), typeof(DuoGridItem));
    }
}