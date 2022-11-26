using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A button which opens a popup with specified content
/// </summary>
[TemplatePart(Name = nameof(PART_PopupButton), Type = typeof(ButtonBase))]
[TemplatePart(Name = nameof(PART_Popup), Type = typeof(Popup))]
public class PopupCard : ContentControl
{
    static PopupCard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(PopupCard), new FrameworkPropertyMetadata(typeof(PopupCard)));
    }

    public PopupCard()
    {
        AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(Button_OnClick));
    }

    private ButtonBase? PART_PopupButton;
    private Popup? PART_Popup;

    public Style PopupButtonStyle
    {
        get => (Style)GetValue(PopupButtonStyleProperty);
        set => SetValue(PopupButtonStyleProperty, value);
    }

    public static readonly DependencyProperty PopupButtonStyleProperty =
        DependencyProperty.Register(nameof(PopupButtonStyle), typeof(Style), typeof(PopupCard));

    public FrameworkElement PopupParentElement
    {
        get => (FrameworkElement)GetValue(PopupParentElementProperty);
        set => SetValue(PopupParentElementProperty, value);
    }

    public static readonly DependencyProperty PopupParentElementProperty =
        DependencyProperty.Register(nameof(PopupParentElement), typeof(FrameworkElement), typeof(PopupCard));

    public PopupOpenDirection OpenDirection
    {
        get => (PopupOpenDirection)GetValue(OpenDirectionProperty);
        set => SetValue(OpenDirectionProperty, value);
    }

    public static readonly DependencyProperty OpenDirectionProperty =
        DependencyProperty.Register(nameof(OpenDirection), typeof(PopupOpenDirection), typeof(PopupCard));

    public override void OnApplyTemplate()
    {
        if (PART_PopupButton != null)
            PART_PopupButton.Click -= PopupButton_OnClick;
        if (PART_Popup != null)
            PART_Popup.Opened -= Popup_OnOpened;

        PART_PopupButton = GetTemplateChild(nameof(PART_PopupButton)) as Button;
        PART_Popup = GetTemplateChild(nameof(PART_Popup)) as Popup;

        if (PART_PopupButton != null)
            PART_PopupButton.Click += PopupButton_OnClick;
        if (PART_Popup != null)
            PART_Popup.Opened += Popup_OnOpened;

        base.OnApplyTemplate();
    }

    private void Button_OnClick(object sender, RoutedEventArgs e)
    {
        // ReSharper disable once PossibleUnintendedReferenceComparison
        if (PART_Popup != null && e.OriginalSource != PART_PopupButton)
            PART_Popup.IsOpen = false;
    }

    private void Popup_OnOpened(object sender, System.EventArgs e)
    {
        if (PART_Popup == null || PART_PopupButton == null)
            return;

        // Position the popup according to the specified direction
        if (PART_Popup.Child is FrameworkElement popupChild && 
            PART_Popup.PlacementTarget is FrameworkElement target)
        {
            PART_Popup.HorizontalOffset = OpenDirection == PopupOpenDirection.Left 
                ? - popupChild.ActualWidth + target.ActualWidth 
                : 0;
        }
    }

    private void PopupButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Toggle if the popup is open
        if (PART_Popup != null)
            PART_Popup.IsOpen ^= true;
    }

    public enum PopupOpenDirection { Left, Right }
}