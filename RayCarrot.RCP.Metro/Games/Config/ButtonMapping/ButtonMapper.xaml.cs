using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for ButtonMapper.xaml
    /// </summary>
    public partial class ButtonMapper : UserControl
    {
        public ButtonMapper()
        {
            InitializeComponent();
            ButtonMapperDataGrid.DataContext = this;
            Loaded += (s, e) => ScrollViewer = this.GetAncestors().FirstOrDefault(x => x is ScrollViewer) as ScrollViewer;
        }

        private ScrollViewer ScrollViewer { get; set; }

        public IEnumerable<ButtonMappingKeyItemViewModel> ItemsSource
        {
            get => (IEnumerable<ButtonMappingKeyItemViewModel>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable<ButtonMappingKeyItemViewModel>), typeof(ButtonMapper));

        private void DataGrid_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Temporary solution for scrolling over data grid
            ScrollViewer?.ScrollToVerticalOffset(ScrollViewer.ContentVerticalOffset - (e.Delta / 2d));
        }

        private void HotKeyBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            ((HotKeyBox)sender).HotKey = new HotKey(e.Key);
        }
    }
}