using MahApps.Metro.IconPacks;
using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.RCP.Core.UI
{
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
    }
}