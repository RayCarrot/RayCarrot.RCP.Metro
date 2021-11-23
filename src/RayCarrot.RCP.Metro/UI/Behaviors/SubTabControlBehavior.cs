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
            // If we hold down shift and at the first index...
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift && ChildTabControl.SelectedIndex == 0)
            {
                if (m.PageTabControl.SelectedIndex == 0)
                    m.PageTabControl.SelectedIndex = m.PageTabControl.Items.Count - 1;
                else
                    m.PageTabControl.SelectedIndex--;

                e.Handled = true;
            }

            // If we do not hold down shift and at the last index...
            else if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift && ChildTabControl.SelectedIndex == ChildTabControl.Items.Count - 1)
            {
                if (m.PageTabControl.SelectedIndex == m.PageTabControl.Items.Count - 1)
                    m.PageTabControl.SelectedIndex = 0;
                else
                    m.PageTabControl.SelectedIndex++;

                e.Handled = true;
            }
        }
    }

    public static readonly DependencyProperty ChildTabControlProperty = DependencyProperty.Register(nameof(ChildTabControl), typeof(TabControl), typeof(SubTabControlBehavior), new FrameworkPropertyMetadata());

    public TabControl ChildTabControl
    {
        get => (TabControl)GetValue(ChildTabControlProperty);
        set => SetValue(ChildTabControlProperty, value);
    }
}