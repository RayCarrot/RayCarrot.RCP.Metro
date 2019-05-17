using System.Windows.Controls;
using RayCarrot.CarrotFramework;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for HelpPage.xaml
    /// </summary>
    public partial class HelpPage : VMUserControl<HelpPageViewModel>
    {
        public HelpPage()
        {
            InitializeComponent();

            RCF.Data.UserLevelChanged += (s, e) =>
            {
                if ((HelpTreeView.SelectedItem as HelpItemViewModel)?.RequiredUserLevel > RCFRCP.Data.UserLevel)
                    ((TreeViewItem)HelpTreeView.ItemContainerGenerator.ContainerFromItem(HelpTreeView.Items[0])).IsSelected = true;
            };
        }
    }
}