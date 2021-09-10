using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A <see cref="ContentControl"/> with support for loading
    /// </summary>
    public class LoaderContentControl : ContentControl
    {
        static LoaderContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LoaderContentControl), new FrameworkPropertyMetadata(typeof(LoaderContentControl)));
        }

        public HorizontalAlignment HorizontalLoadingIconAlignment
        {
            get => (HorizontalAlignment)GetValue(HorizontalLoadingIconAlignmentProperty);
            set => SetValue(HorizontalLoadingIconAlignmentProperty, value);
        }

        public static readonly DependencyProperty HorizontalLoadingIconAlignmentProperty = DependencyProperty.Register(nameof(HorizontalLoadingIconAlignment), typeof(HorizontalAlignment), typeof(LoaderContentControl), new PropertyMetadata(HorizontalAlignment.Center));

        public VerticalAlignment VerticalLoadingIconAlignment
        {
            get => (VerticalAlignment)GetValue(VerticalLoadingIconAlignmentProperty);
            set => SetValue(VerticalLoadingIconAlignmentProperty, value);
        }

        public static readonly DependencyProperty VerticalLoadingIconAlignmentProperty = DependencyProperty.Register(nameof(VerticalLoadingIconAlignment), typeof(VerticalAlignment), typeof(LoaderContentControl), new PropertyMetadata(VerticalAlignment.Center));

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(LoaderContentControl), new PropertyMetadata(false));
    }
}