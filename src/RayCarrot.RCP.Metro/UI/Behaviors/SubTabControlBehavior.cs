using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Behavior for handling tab navigation for a child tab control in the main window
/// </summary>
public class SubTabControlBehavior : Behavior<UIElement>
{
    protected override void OnAttached()
    {
        AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
    }

    private void AssociatedObject_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (
            // If tab key is pressed...
            e.Key == Key.Tab &&
            // and control is pressed...
            (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
            // and the main window is of the right type
            App.Current.MainWindow is MainWindow m)
        {
            int firstValidChildTabIndex = GetNextValidTabIndex(ChildTabControl, 0, 1);
            int lastValidChildTabIndex = GetNextValidTabIndex(ChildTabControl, ChildTabControl.Items.Count - 1, -1);

            // If we hold down shift and at the first index...
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift && 
                ChildTabControl.SelectedIndex == firstValidChildTabIndex)
            {
                if (m.PageTabControl.SelectedIndex == 0)
                    m.PageTabControl.SelectedIndex = GetNextValidTabIndex(m.PageTabControl, m.PageTabControl.Items.Count - 1, -1);
                else
                    m.PageTabControl.SelectedIndex = GetNextValidTabIndex(m.PageTabControl, m.PageTabControl.SelectedIndex - 1, -1);

                e.Handled = true;
            }

            // If we do not hold down shift and at the last index...
            else if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift && ChildTabControl.SelectedIndex == lastValidChildTabIndex)
            {
                if (m.PageTabControl.SelectedIndex == m.PageTabControl.Items.Count - 1)
                    m.PageTabControl.SelectedIndex = GetNextValidTabIndex(m.PageTabControl, 0, 1);
                else
                    m.PageTabControl.SelectedIndex = GetNextValidTabIndex(m.PageTabControl, m.PageTabControl.SelectedIndex + 1, 1);

                e.Handled = true;
            }
        }
    }

    private int GetNextValidTabIndex(TabControl tabControl, int startIndex, int step)
    {
        int index = startIndex;

        while (!((TabItem)tabControl.ItemContainerGenerator.ContainerFromIndex(index)).IsEnabled ||
               ((TabItem)tabControl.ItemContainerGenerator.ContainerFromIndex(index)).Visibility != Visibility.Visible)
            index += step;

        return index;
    }

    public static readonly DependencyProperty ChildTabControlProperty = DependencyProperty.Register(nameof(ChildTabControl), typeof(TabControl), typeof(SubTabControlBehavior), new FrameworkPropertyMetadata());

    public TabControl ChildTabControl
    {
        get => (TabControl)GetValue(ChildTabControlProperty);
        set => SetValue(ChildTabControlProperty, value);
    }
}