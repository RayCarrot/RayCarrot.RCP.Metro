using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A <see cref="ContentControl"/> with support for loading
/// </summary>
public class LoaderContentControl : ContentControl
{
    static LoaderContentControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LoaderContentControl), new FrameworkPropertyMetadata(typeof(LoaderContentControl)));
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty TextProperty = 
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(LoaderContentControl), new PropertyMetadata(null));

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public static readonly DependencyProperty IsLoadingProperty = 
        DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(LoaderContentControl), new PropertyMetadata(false));

    public bool IsIndeterminate
    {
        get => (bool)GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    public static readonly DependencyProperty IsIndeterminateProperty = 
        DependencyProperty.Register(nameof(IsIndeterminate), typeof(bool), typeof(LoaderContentControl), new PropertyMetadata(true));
}