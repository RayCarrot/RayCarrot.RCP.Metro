using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for UtilitiesContainer.xaml
    /// </summary>
    public partial class UtilitiesContainer : UserControl
    {
        public UtilitiesContainer()
        {
            InitializeComponent();
            DataContextRoot.DataContext = this;
        }

        public IEnumerable<IRCPUtility> Utilities
        {
            get => (IEnumerable<IRCPUtility>)GetValue(UtilitiesProperty);
            set => SetValue(UtilitiesProperty, value);
        }

        public static readonly DependencyProperty UtilitiesProperty = DependencyProperty.Register(nameof(Utilities), typeof(IEnumerable<IRCPUtility>), typeof(UtilitiesContainer), new PropertyMetadata(new IRCPUtility[0]));
    }
}