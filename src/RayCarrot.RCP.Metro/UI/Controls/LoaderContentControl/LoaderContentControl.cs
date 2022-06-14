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

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty ValueProperty = 
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(LoaderContentControl), new PropertyMetadata(0d));

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public static readonly DependencyProperty MinimumProperty = 
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(LoaderContentControl), new PropertyMetadata(0d));

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public static readonly DependencyProperty MaximumProperty = 
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(LoaderContentControl), new PropertyMetadata(100d));
}