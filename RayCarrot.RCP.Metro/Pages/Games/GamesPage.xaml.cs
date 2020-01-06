using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for GamesPage.xaml
    /// </summary>
    public partial class GamesPage : BasePage<GamesPageViewModel>
    {
        public GamesPage()
        {
            InitializeComponent();
        }

        private void UIElement_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (
                // If tab key is pressed...
                e.Key == Key.Tab &&
                // and control is pressed...
                (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                // and the main window is of the right type
                App.Current.MainWindow is MainWindow m)
            {
                // If we hold down shift and at the first index...
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift && GamesTabControl.SelectedIndex == 0)
                {
                    if (m.PageTabControl.SelectedIndex == 0)
                        m.PageTabControl.SelectedIndex = m.PageTabControl.Items.Count - 1;
                    else
                        m.PageTabControl.SelectedIndex--;

                    e.Handled = true;
                }

                // If we do not hold down shift and at the last index...
                else if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift && GamesTabControl.SelectedIndex == GamesTabControl.Items.Count - 1)
                {
                    if (m.PageTabControl.SelectedIndex == m.PageTabControl.Items.Count - 1)
                        m.PageTabControl.SelectedIndex = 0;
                    else
                        m.PageTabControl.SelectedIndex++;

                    e.Handled = true;
                }
            }
        }
    }
}