using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for Utility_Ray1Editor_UI.xaml
    /// </summary>
    public partial class Utility_Ray1Editor_UI : UserControl
    {
        public Utility_Ray1Editor_UI(Utility_Ray1Editor_ViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
        }

        public Utility_Ray1Editor_ViewModel ViewModel
        {
            get => (Utility_Ray1Editor_ViewModel)DataContext;
            set => DataContext = value;
        }
    }
}