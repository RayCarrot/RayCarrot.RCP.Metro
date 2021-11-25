#nullable disable
using System.Linq;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace RayCarrot.RCP.Metro;

public abstract class AttachableForStyleBehavior<TComponent, TBehavior> : Behavior<TComponent>
    where TComponent : DependencyObject
    where TBehavior : AttachableForStyleBehavior<TComponent, TBehavior>, new()
{
    public bool IsEnabledForStyle
    {
        get => (bool)GetValue(IsEnabledForStyleProperty);
        set => SetValue(IsEnabledForStyleProperty, value);
    }

    public static DependencyProperty IsEnabledForStyleProperty =
        DependencyProperty.RegisterAttached(nameof(IsEnabledForStyle), typeof(bool),
            typeof(AttachableForStyleBehavior<TComponent, TBehavior>), new FrameworkPropertyMetadata(false, OnIsEnabledForStyleChanged));

    private static void OnIsEnabledForStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement uie) 
            return;
            
        BehaviorCollection behColl = Interaction.GetBehaviors(uie);
        var existingBehavior = behColl.FirstOrDefault(b => b.GetType() == typeof(TBehavior)) as TBehavior;

        if ((bool)e.NewValue == false && existingBehavior != null)
            behColl.Remove(existingBehavior);

        else if ((bool)e.NewValue && existingBehavior == null)
            behColl.Add(new TBehavior());
    }
}