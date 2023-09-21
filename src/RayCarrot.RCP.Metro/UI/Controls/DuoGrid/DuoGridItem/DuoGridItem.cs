#nullable disable
using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A duo grid item to be used in a <see cref="DuoGrid"/>
/// </summary>
[TemplatePart(Name = nameof(PART_CopyValueMenuItem), Type = typeof(MenuItem))]
public class DuoGridItem : Control
{
    static DuoGridItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DuoGridItem), new FrameworkPropertyMetadata(typeof(DuoGridItem)));
    }

    private MenuItem PART_CopyValueMenuItem;

    /// <summary>
    /// The header
    /// </summary>
    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(string), typeof(DuoGridItem));

    /// <summary>
    /// The text to display
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(DuoGridItem));

    /// <summary>
    /// The minimum user level for this item
    /// </summary>
    public UserLevel MinUserLevel
    {
        get => (UserLevel)GetValue(MinUserLevelProperty);
        set => SetValue(MinUserLevelProperty, value);
    }

    public static readonly DependencyProperty MinUserLevelProperty = DependencyProperty.Register(nameof(MinUserLevel), typeof(UserLevel), typeof(DuoGridItem));

    private void CopyValueMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(Text);
    }

    public override void OnApplyTemplate()
    {
        if (PART_CopyValueMenuItem != null)
            PART_CopyValueMenuItem.Click -= CopyValueMenuItem_OnClick;

        PART_CopyValueMenuItem = GetTemplateChild(nameof(PART_CopyValueMenuItem)) as MenuItem;

        if (PART_CopyValueMenuItem != null)
            PART_CopyValueMenuItem.Click += CopyValueMenuItem_OnClick;

        base.OnApplyTemplate();
    }
}