using System.ComponentModel;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Provides attached properties for tagging <see cref="FrameworkElement"/> objects with a minimum <see cref="UserLevel"/>
/// </summary>
public static class UserLevelAssist
{
    #region MinUserLevel

    /// <summary>
    /// Gets the minimum <see cref="UserLevel"/> from a <see cref="DependencyObject"/>
    /// </summary>
    /// <param name="obj">The object to get the property from</param>
    /// <returns>The property</returns>
    public static UserLevel GetMinUserLevel(DependencyObject obj) => (UserLevel)obj.GetValue(MinUserLevelProperty);

    /// <summary>
    /// Sets the minimum <see cref="UserLevel"/> for a <see cref="DependencyObject"/>
    /// </summary>
    /// <param name="obj">The object to set the property on</param>
    /// <param name="value">The property to set</param>
    public static void SetMinUserLevel(DependencyObject obj, UserLevel value) => obj.SetValue(MinUserLevelProperty, value);

    /// <summary>
    /// The property for the minimum <see cref="UserLevel"/>
    /// </summary>
    public static readonly DependencyProperty MinUserLevelProperty = DependencyProperty.RegisterAttached("MinUserLevel", typeof(UserLevel), typeof(UserLevelAssist), new PropertyMetadata(UserLevel.Normal, MinUserLevelChanged));

    #endregion

    #region Behavior

    /// <summary>
    /// Gets the requested mode from a <see cref="DependencyObject"/>
    /// </summary>
    /// <param name="obj">The object to get the property from</param>
    /// <returns>The property</returns>
    public static UserLevelAssistMode GetMode(DependencyObject obj) => (UserLevelAssistMode)obj.GetValue(ModeProperty);

    /// <summary>
    /// Sets the requested mode for a <see cref="DependencyObject"/>
    /// </summary>
    /// <param name="obj">The object to set the property on</param>
    /// <param name="value">The property to set</param>
    public static void SetMode(DependencyObject obj, UserLevelAssistMode value) => obj.SetValue(ModeProperty, value);

    /// <summary>
    /// The property for the requested mode
    /// </summary>
    public static readonly DependencyProperty ModeProperty = DependencyProperty.RegisterAttached("Mode", typeof(UserLevelAssistMode), typeof(UserLevelAssist), new PropertyMetadata(UserLevelAssistMode.Collapse, ModeChanged));

    #endregion

    #region Private Event Handlers

    private static void ModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Return if in design mode
        if (DesignerProperties.GetIsInDesignMode(d))
            return;

        // Make sure we've got a UI Element
        if (d is not FrameworkElement uiElement)
            return;

        // Revert values to default
        uiElement.IsEnabled = true;
        uiElement.Visibility = Visibility.Visible;

        // Refresh the element
        RefreshElement(uiElement);
    }

    private static void MinUserLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Return if in design mode
        if (DesignerProperties.GetIsInDesignMode(d))
            return;

        // Make sure we've got a UI Element
        if (d is not FrameworkElement uIElement)
            return;

        // Create a weak reference
        var element = new WeakReference<FrameworkElement>(uIElement);

        // Subscribe to when the current user level changes
        Services.InstanceData.UserLevelChanged += RefreshItem;

        // Refresh the element
        RefreshElement(uIElement);

        void RefreshItem(object ss, EventArgs ee)
        {
            if (element.TryGetTarget(out FrameworkElement ue))
                RefreshElement(ue);
            else
                Services.InstanceData.UserLevelChanged -= RefreshItem;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Refreshes a element
    /// </summary>
    /// <param name="element">The element to refresh</param>
    public static void RefreshElement(FrameworkElement element)
    {
        var b = GetMode(element);

        if (b == UserLevelAssistMode.Collapse)
            element.Visibility = GetMinUserLevel(element) <= Services.InstanceData.CurrentUserLevel ? Visibility.Visible : Visibility.Collapsed;
        else if (b == UserLevelAssistMode.Disable)
            element.IsEnabled = GetMinUserLevel(element) <= Services.InstanceData.CurrentUserLevel;
    }

    #endregion
}