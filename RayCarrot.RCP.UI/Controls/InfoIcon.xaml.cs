using System;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.UI
{
    /// <summary>
    /// Interaction logic for InfoIcon.xaml
    /// </summary>
    public partial class InfoIcon : UserControl
    {
        public InfoIcon()
        {
            InitializeComponent();
            HelpIconRoot.DataContext = this;
        }

        /// <summary>
        /// The icon kind
        /// </summary>
        public PackIconMaterialKind IconKind
        {
            get => (PackIconMaterialKind)GetValue(IconKindProperty);
            set => SetValue(IconKindProperty, value);
        }

        public static readonly DependencyProperty IconKindProperty = DependencyProperty.Register(nameof(IconKind), typeof(PackIconMaterialKind), typeof(InfoIcon), new PropertyMetadata(PackIconMaterialKind.HelpCircleOutline));

        /// <summary>
        /// The text to show
        /// </summary>
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(InfoIcon), new PropertyMetadata(String.Empty));
    }
}