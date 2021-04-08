using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for R1PasswordGeneratorUtilityUI.xaml
    /// </summary>
    public partial class R1PasswordGeneratorUtilityUI : UserControl
    {
        public R1PasswordGeneratorUtilityUI(R1PasswordGeneratorUtilityViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
        }

        public R1PasswordGeneratorUtilityViewModel ViewModel
        {
            get => (R1PasswordGeneratorUtilityViewModel)DataContext;
            set => DataContext = value;
        }
    }
}