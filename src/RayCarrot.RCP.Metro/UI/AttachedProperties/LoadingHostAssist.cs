﻿using System.Windows;
using System.Windows.Data;

namespace RayCarrot.RCP.Metro;

public static class LoadingHostAssist
{
    public static LoaderViewModel GetViewModel(LoadingHost obj) => (LoaderViewModel)obj.GetValue(ViewModelProperty);

    public static void SetViewModel(LoadingHost obj, LoaderViewModel value) => obj.SetValue(ViewModelProperty, value);

    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.RegisterAttached(
        name: "ViewModel",
        propertyType: typeof(LoaderViewModel),
        ownerType: typeof(LoadingHostAssist), 
        defaultMetadata: new FrameworkPropertyMetadata(OnViewModelChanged));

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        FrameworkElement f = (FrameworkElement)d;

        f.SetBinding(LoadingHost.IsLoadingProperty, 
            new Binding(nameof(LoaderViewModel.State))
            {
                Source = e.NewValue,
                Converter = new IsNotNullConverter()
            });
        f.SetBinding(LoadingHost.TextProperty, 
            new Binding($"{nameof(LoaderViewModel.State)}.{nameof(LoadStateViewModel.Status)}")
            {
                Source = e.NewValue
            });
        f.SetBinding(LoadingHost.IsIndeterminateProperty, 
            new Binding($"{nameof(LoaderViewModel.State)}.{nameof(LoadStateViewModel.HasProgress)}")
            {
                Source = e.NewValue,
                Converter = new InvertedBooleanConverter()
            });
        f.SetBinding(LoadingHost.MinimumProperty, 
            new Binding($"{nameof(LoaderViewModel.State)}.{nameof(LoadStateViewModel.MinProgress)}")
            {
                Source = e.NewValue
            });
        f.SetBinding(LoadingHost.MaximumProperty, 
            new Binding($"{nameof(LoaderViewModel.State)}.{nameof(LoadStateViewModel.MaxProgress)}")
            {
                Source = e.NewValue
            });
        f.SetBinding(LoadingHost.ValueProperty, 
            new Binding($"{nameof(LoaderViewModel.State)}.{nameof(LoadStateViewModel.CurrentProgress)}")
            {
                Source = e.NewValue
            });
    }
}