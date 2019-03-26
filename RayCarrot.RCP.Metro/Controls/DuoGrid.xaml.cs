using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for DuoGrid.xaml
    /// </summary>
    public partial class DuoGrid : UserControl
    {
        public DuoGrid()
        {
            InitializeComponent();
            DuoGridItemsControl.DataContext = this;
        }

        /// <summary>
        /// The items to show in the grid
        /// </summary>
        public IEnumerable<DuoGridItemViewModel> Items
        {
            get => (IEnumerable<DuoGridItemViewModel>)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(nameof(Items), typeof(IEnumerable<DuoGridItemViewModel>), typeof(DuoGrid));
    }
}