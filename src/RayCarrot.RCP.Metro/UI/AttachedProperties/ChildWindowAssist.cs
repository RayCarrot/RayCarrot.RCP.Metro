using System.Windows;
using MahApps.Metro.SimpleChildWindow;

namespace RayCarrot.RCP.Metro;

public static class ChildWindowAssist
{
    #region IsCloseButtonEnabled

    public static bool GetIsCloseButtonEnabled(ChildWindow obj) => (bool)obj.GetValue(IsCloseButtonEnabledProperty);

    public static void SetIsCloseButtonEnabled(ChildWindow obj, bool value) => obj.SetValue(IsCloseButtonEnabledProperty, value);

    public static readonly DependencyProperty IsCloseButtonEnabledProperty = DependencyProperty.RegisterAttached(
        name: "IsCloseButtonEnabled",
        propertyType: typeof(bool),
        ownerType: typeof(ChildWindowAssist),
        defaultMetadata: new FrameworkPropertyMetadata(defaultValue: true));

    #endregion
}