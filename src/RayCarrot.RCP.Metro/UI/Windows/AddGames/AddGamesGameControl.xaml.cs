using System;
using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for AddGamesGameControl.xaml
    /// </summary>
    public partial class AddGamesGameControl : UserControl
    {
        public AddGamesGameControl()
        {
            InitializeComponent();
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            PurchaseLinksPopup.IsOpen ^= true;
        }

        private void PurchaseLinksPopup_OnOpened(object sender, EventArgs e)
        {
            PurchaseLinksPopup.HorizontalOffset = -PopupCard.ActualWidth + PurchaseLinksGrid.ActualWidth;
        }
    }
}