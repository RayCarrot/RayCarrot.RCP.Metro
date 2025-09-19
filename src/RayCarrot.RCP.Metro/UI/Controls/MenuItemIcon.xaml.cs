#nullable disable
using MahApps.Metro.IconPacks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for MenuItemIcon.xaml
/// </summary>
public partial class MenuItemIcon : MenuItem
{
    public MenuItemIcon()
    {
        InitializeComponent();
        PackIcon.DataContext = this;
    }

    public PackIconMaterialKind IconKind
    {
        get => (PackIconMaterialKind)GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }

    public static readonly DependencyProperty IconKindProperty = DependencyProperty.Register(nameof(IconKind), typeof(PackIconMaterialKind), typeof(MenuItemIcon), new PropertyMetadata(PackIconMaterialKind.None));

    public Brush IconForeground
    {
        get => (Brush)GetValue(IconForegroundProperty);
        set => SetValue(IconForegroundProperty, value);
    }

    public static readonly DependencyProperty IconForegroundProperty = DependencyProperty.Register(nameof(IconForeground), typeof(Brush), typeof(MenuItemIcon));
}