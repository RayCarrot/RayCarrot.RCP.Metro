using System;
using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for HelpIcon.xaml
    /// </summary>
    public partial class HelpIcon : UserControl
    {
        public HelpIcon()
        {
            InitializeComponent();
            HelpIconRoot.DataContext = this;
        }

        /// <summary>
        /// The text to show
        /// </summary>
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(HelpIcon), new PropertyMetadata(String.Empty));
    }
}