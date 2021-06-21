using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for Ray1EditorUtilityUI.xaml
    /// </summary>
    public partial class Ray1EditorUtilityUI : UserControl
    {
        public Ray1EditorUtilityUI(Ray1EditorUtilityViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
        }

        public Ray1EditorUtilityViewModel ViewModel
        {
            get => (Ray1EditorUtilityViewModel)DataContext;
            set => DataContext = value;
        }
    }
}