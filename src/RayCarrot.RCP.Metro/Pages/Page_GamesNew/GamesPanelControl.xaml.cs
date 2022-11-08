using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for GamesPanelControl.xaml
    /// </summary>
    public partial class GamesPanelControl : UserControl
    {
        public GamesPanelControl()
        {
            InitializeComponent();
        }

        public Page_GamesNew_ViewModel ViewModel => (Page_GamesNew_ViewModel)DataContext;

        private void GamesListBox_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Redirect the mouse wheel movement to allow scrolling
            MouseWheelEventArgs eventArg = new(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = MouseWheelEvent,
                Source = e.Source
            };

            GamesPanelScrollViewer?.RaiseEvent(eventArg);
            e.Handled = true;
        }

        private void GameGroupListBoxItem_OnSelected(object sender, RoutedEventArgs e)
        {
            if (sender is not ListBoxItem { DataContext: InstalledGameViewModel vm }) 
                return;
            
            ViewModel.SelectedInstalledGame = vm;

            foreach (GameCategoryViewModel gameCategoryViewModel in ViewModel.GameCategories)
            {
                foreach (GameGroupViewModel gameGroupViewModel in gameCategoryViewModel.GameGroups)
                {
                    gameGroupViewModel.SelectedInstalledGame = null;
                }
            }
        }
    }
}
