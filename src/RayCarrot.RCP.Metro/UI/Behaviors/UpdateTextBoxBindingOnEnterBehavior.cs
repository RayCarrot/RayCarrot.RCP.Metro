using Microsoft.Xaml.Behaviors;
using System.Windows.Controls;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

public class UpdateTextBoxBindingOnEnterBehavior : Behavior<TextBox>
{
    protected override void OnAttached()
    {
        AssociatedObject.KeyDown += AssociatedObject_KeyDown;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
    }

    private void AssociatedObject_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            AssociatedObject.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
    }
}