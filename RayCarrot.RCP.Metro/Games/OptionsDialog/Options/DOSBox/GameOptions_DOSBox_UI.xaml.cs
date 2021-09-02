using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for GameOptions_DOSBox_UI.xaml
    /// </summary>
    public partial class GameOptions_DOSBox_UI : UserControl
    {
        public GameOptions_DOSBox_UI(Games game)
        {
            InitializeComponent();

            ViewModel = new GameOptions_DOSBox_ViewModel(game);
            DataContext = ViewModel;
        }

        public GameOptions_DOSBox_ViewModel ViewModel { get; }
    }
}