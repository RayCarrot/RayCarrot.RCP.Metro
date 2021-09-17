using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for AppNewsDialog.xaml
    /// </summary>
    public partial class AppNewsDialog : WindowContentControl
    {
        public AppNewsDialog()
        {
            InitializeComponent();
        }

        public override IWindowControl.WindowResizeMode ResizeMode => IWindowControl.WindowResizeMode.ForceResizable;

        protected override void WindowAttached()
        {
            base.WindowAttached();

            WindowInstance.Title = Metro.Resources.AppNews_Title;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            WindowInstance.Close();
        }
    }
}