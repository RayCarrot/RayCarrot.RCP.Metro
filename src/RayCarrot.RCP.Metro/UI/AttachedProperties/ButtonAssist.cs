using System.Windows;
using System.Windows.Controls.Primitives;

namespace RayCarrot.RCP.Metro;

public static class ButtonAssist
{
    #region Is Selected

    public static bool GetIsSelected(ButtonBase obj) => (bool)obj.GetValue(IsSelectedProperty);

    public static void SetIsSelected(ButtonBase obj, bool value) => obj.SetValue(IsSelectedProperty, value);

    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.RegisterAttached(
        name: "IsSelected",
        propertyType: typeof(bool),
        ownerType: typeof(ButtonAssist));

    #endregion
}