using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.UI
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
        
        /// <summary>
        /// Indicates the icon visibility
        /// </summary>
        public Visibility IconVisibility
        {
            get => (Visibility)GetValue(IconVisibilityProperty);
            set => SetValue(IconVisibilityProperty, value);
        }

        /// <summary>
        /// Dependency property for <see cref="IconVisibility"/>
        /// </summary>
        public static readonly DependencyProperty IconVisibilityProperty = DependencyProperty.Register(nameof(IconVisibility), typeof(Visibility), typeof(RCPTabItem), new PropertyMetadata(Visibility.Visible));
    }
}