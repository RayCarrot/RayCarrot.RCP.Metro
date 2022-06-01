#nullable disable
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A duo grid containing a collection of <see cref="DuoGridItemViewModel"/>
/// </summary>
public class DuoGrid : Control
{
    /// <summary>
    /// The minimum user level for this item
    /// </summary>
    public IEnumerable<DuoGridItemViewModel> ItemsSource
    {
        get => (IEnumerable<DuoGridItemViewModel>)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty ItemsSourceProperty = 
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable<DuoGridItemViewModel>), typeof(DuoGrid));
}