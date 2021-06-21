using System.Windows.Input;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Ray1Editor external tool
    /// </summary>
    public class Ray1EditorUtilityViewModel : BaseRCPViewModel
    {
        public Ray1EditorUtilityViewModel()
        {
            OpenHomePageCommand = new RelayCommand(OpenHomePage);
        }

        public ICommand OpenHomePageCommand { get; }

        public string HomePageURL => "https://raym.app/ray1editor/";

        public void OpenHomePage()
        {
            App.OpenUrl(HomePageURL);
        }
    }
}