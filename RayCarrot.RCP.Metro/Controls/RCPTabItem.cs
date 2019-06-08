using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A Rayman Control Panel tab item with an icon
    /// </summary>
    public class RCPTabItem : TabItem
    {
        /// <summary>
        /// Indicates the icon kind to use for the tab header
        /// </summary>
        public PackIconMaterialKind IconKind
        {
            get => (PackIconMaterialKind)GetValue(IconKindProperty);
            set => SetValue(IconKindProperty, value);
        }

        /// <summary>
        /// Dependency property for <see cref="IconKind"/>
        /// </summary>
        public static readonly DependencyProperty IconKindProperty = DependencyProperty.Register(nameof(IconKind), typeof(PackIconMaterialKind), typeof(RCPTabItem));

        /// <summary>
        /// Indicates the header font size
        /// </summary>
        public double HeaderFontSize
        {
            get => (double)GetValue(HeaderFontSizeProperty);
            set => SetValue(HeaderFontSizeProperty, value);
        }

        /// <summary>
        /// Dependency property for <see cref="IconKind"/>
        /// </summary>
        public static readonly DependencyProperty HeaderFontSizeProperty = DependencyProperty.Register(nameof(HeaderFontSize), typeof(double), typeof(RCPTabItem));
    }
}