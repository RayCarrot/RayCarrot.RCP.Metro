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

        // Position the popup to the left
        if (PART_Popup.Child is FrameworkElement popupChild)
            PART_Popup.HorizontalOffset = -popupChild.ActualWidth + PART_PopupButton.ActualWidth;
    }

    private void PopupButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Toggle if the popup is open
        if (PART_Popup != null)
            PART_Popup.IsOpen ^= true;
    }
}