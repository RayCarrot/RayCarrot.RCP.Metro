using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A <see cref="ContentControl"/> with support for loading
/// </summary>
[TemplatePart(Name = nameof(PART_CancelButton), Type = typeof(ButtonBase))]
public class LoadingHost : ContentControl
{
    static LoadingHost()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadingHost), new FrameworkPropertyMetadata(typeof(LoadingHost)));
    }

    private ButtonBase? PART_CancelButton;

    public override void OnApplyTemplate()
    {
        if (PART_CancelButton != null)
            PART_CancelButton.Click -= CancelButtonOnClick;

        PART_CancelButton = GetTemplateChild(nameof(PART_CancelButton)) as Button;

        if (PART_CancelButton != null)
            PART_CancelButton.Click += CancelButtonOnClick;

        base.OnApplyTemplate();
    }

    private void CancelButtonOnClick(object sender, RoutedEventArgs e)
    {
        if (CancelCommand != null && CancelCommand.CanExecute(CancelCommandParameter))
            CancelCommand.Execute(CancelCommandParameter);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty TextProperty = 
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(LoadingHost), new PropertyMetadata(null));

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public static readonly DependencyProperty IsLoadingProperty = 
        DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(LoadingHost), new PropertyMetadata(false));

    public bool IsIndeterminate
    {
        get => (bool)GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    public static readonly DependencyProperty IsIndeterminateProperty = 
        DependencyProperty.Register(nameof(IsIndeterminate), typeof(bool), typeof(LoadingHost), new PropertyMetadata(true));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty ValueProperty = 
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(LoadingHost), new PropertyMetadata(0d));

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public static readonly DependencyProperty MinimumProperty = 
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(LoadingHost), new PropertyMetadata(0d));

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public static readonly DependencyProperty MaximumProperty = 
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(LoadingHost), new PropertyMetadata(100d));

    public bool CanCancel
    {
        get => (bool)GetValue(CanCancelProperty);
        set => SetValue(CanCancelProperty, value);
    }

    public static readonly DependencyProperty CanCancelProperty = 
        DependencyProperty.Register(nameof(CanCancel), typeof(bool), typeof(LoadingHost), new PropertyMetadata(false));

    public LoadingHostState State
    {
        get => (LoadingHostState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public static readonly DependencyProperty StateProperty = 
        DependencyProperty.Register(nameof(State), typeof(LoadingHostState), typeof(LoadingHost), new PropertyMetadata(LoadingHostState.Normal));

    public ICommand? CancelCommand
    {
        get => (ICommand?)GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }

    public static readonly DependencyProperty CancelCommandProperty = 
        DependencyProperty.Register(nameof(CancelCommand), typeof(ICommand), typeof(LoadingHost));

    public object? CancelCommandParameter
    {
        get => GetValue(CancelCommandParameterProperty);
        set => SetValue(CancelCommandParameterProperty, value);
    }

    public static readonly DependencyProperty CancelCommandParameterProperty = 
        DependencyProperty.Register(nameof(CancelCommandParameter), typeof(object), typeof(LoadingHost));
}