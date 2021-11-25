#nullable disable
using System.Windows.Controls;
using MahApps.Metro.Controls;

namespace RayCarrot.RCP.Metro;

public class DynamicTabAnimationBehavior : AttachableForStyleBehavior<MetroAnimatedTabControl, DynamicTabAnimationBehavior>
{
    protected override void OnAttached()
    {
        AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
    }

    private int prevSelectedIndex;

    private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AssociatedObject.SelectedIndex == -1 || AssociatedObject.SelectedIndex == prevSelectedIndex) 
            return;

        if (Services.Data.UI_EnableAnimations)
            TabControlHelper.SetTransition(AssociatedObject,
                prevSelectedIndex > AssociatedObject.SelectedIndex ? TransitionType.Right : TransitionType.Left);
        else
            TabControlHelper.SetTransition(AssociatedObject, TransitionType.Normal);

        prevSelectedIndex = AssociatedObject.SelectedIndex;
    }
}