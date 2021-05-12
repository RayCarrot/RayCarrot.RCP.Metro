using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for DOSBoxOptions.xaml
    /// </summary>
    public partial class DOSBoxOptions : UserControl
    {
        public DOSBoxOptions(Games game)
        {
            InitializeComponent();

            ViewModel = new DOSBoxOptionsViewModel(game);
            DataContext = ViewModel;
        }

        public DOSBoxOptionsViewModel ViewModel { get; }
    }
}