using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for IconButton.xaml
/// </summary>
public partial class IconButton : Button
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public IconButton()
    {
        InitializeComponent();
        PackIconMaterial.DataContext = this;
    }

    /// <summary>
    /// The kind of icon to show
    /// </summary>
    public PackIconMaterialKind IconKind
    {
        get => (PackIconMaterialKind)GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }

    public static readonly DependencyProperty IconKindProperty = DependencyProperty.Register(nameof(IconKind), typeof(PackIconMaterialKind), typeof(IconButton));
}