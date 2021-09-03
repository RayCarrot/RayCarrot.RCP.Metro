using System.Windows;
using System.Windows.Media;
using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro
{
    public static class TabItem
    {
        #region Icon Kind

        public static PackIconMaterialKind GetIconKind(System.Windows.Controls.TabItem obj) => (PackIconMaterialKind)obj.GetValue(IconKindProperty);

        public static void SetIconKind(System.Windows.Controls.TabItem obj, PackIconMaterialKind value) => obj.SetValue(IconKindProperty, value);

        public static readonly DependencyProperty IconKindProperty = DependencyProperty.RegisterAttached(
            name: "IconKind",
            propertyType: typeof(PackIconMaterialKind),
            ownerType: typeof(TabItem));

        #endregion

        #region Header Font Size

        public static double GetHeaderFontSize(System.Windows.Controls.TabItem obj) => (double)obj.GetValue(HeaderFontSizeProperty);

        public static void SetHeaderFontSize(System.Windows.Controls.TabItem obj, double value) => obj.SetValue(HeaderFontSizeProperty, value);

        public static readonly DependencyProperty HeaderFontSizeProperty = DependencyProperty.RegisterAttached(
            name: "HeaderFontSize",
            propertyType: typeof(double),
            ownerType: typeof(TabItem));

        #endregion

        #region Icon Visibility

        public static Visibility GetIconVisibility(System.Windows.Controls.TabItem obj) => (Visibility)obj.GetValue(IconVisibilityProperty);

        public static void SetIconVisibility(System.Windows.Controls.TabItem obj, Visibility value) => obj.SetValue(IconVisibilityProperty, value);

        public static readonly DependencyProperty IconVisibilityProperty = DependencyProperty.RegisterAttached(
            name: "IconVisibility",
            propertyType: typeof(Visibility),
            ownerType: typeof(TabItem));

        #endregion

        #region Icon Foreground

        public static Brush GetIconForeground(System.Windows.Controls.TabItem obj) => (Brush)obj.GetValue(IconForegroundProperty);

        public static void SetIconForeground(System.Windows.Controls.TabItem obj, Brush value) => obj.SetValue(IconForegroundProperty, value);

        public static readonly DependencyProperty IconForegroundProperty = DependencyProperty.RegisterAttached(
            name: "IconForeground",
            propertyType: typeof(Brush),
            ownerType: typeof(TabItem));

        #endregion
    }
}