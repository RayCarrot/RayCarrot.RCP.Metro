#nullable disable
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for ButtonMapper.xaml
/// </summary>
public partial class ButtonMapper : UserControl
{
    public ButtonMapper()
    {
        InitializeComponent();
        ButtonMapperDataGrid.DataContext = this;
        Loaded += (_, _) => ScrollViewer = this.GetAncestors().FirstOrDefault(x => x is ScrollViewer) as ScrollViewer;
    }

    private ScrollViewer ScrollViewer { get; set; }

    public IEnumerable<ButtonMapperKeyItemViewModel> ItemsSource
    {
        get => (IEnumerable<ButtonMapperKeyItemViewModel>)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable<ButtonMapperKeyItemViewModel>), typeof(ButtonMapper));

    private void DataGrid_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Redirect the mouse wheel movement to allow scrolling

        var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
        {
            RoutedEvent = MouseWheelEvent, Source = e.Source
        };

        ScrollViewer?.RaiseEvent(eventArg);
        e.Handled = true;
    }

    private void HotKeyBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        ((HotKeyBox)sender).HotKey = new HotKey(e.Key);
    }
}