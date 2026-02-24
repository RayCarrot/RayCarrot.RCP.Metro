using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Pages.Utilities;

/// <summary>
/// View model for the Ray1Editor external tool
/// </summary>
public class Ray1EditorUtilityViewModel : UtilityViewModel
{
    public Ray1EditorUtilityViewModel()
    {
        OpenHomePageCommand = new RelayCommand(OpenHomePage);
    }

    public ICommand OpenHomePageCommand { get; }

    public override LocalizedString DisplayHeader => new ResourceLocString(nameof(Resources.Utilities_R1E_Header));
    public override GenericIconKind Icon => GenericIconKind.Utilities_Ray1Editor;

    public string HomePageURL => "https://github.com/RayCarrot/Ray1Editor";

    public void OpenHomePage()
    {
        Services.App.OpenUrl(HomePageURL);
    }
}