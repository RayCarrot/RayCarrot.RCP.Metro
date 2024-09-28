﻿using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Pages.Utilities;

/// <summary>
/// View model for the Ray1Editor external tool
/// </summary>
public class Utility_Ray1Editor_ViewModel : UtilityViewModel
{
    public Utility_Ray1Editor_ViewModel()
    {
        OpenHomePageCommand = new RelayCommand(OpenHomePage);
    }

    public ICommand OpenHomePageCommand { get; }

    public override LocalizedString DisplayHeader => new ResourceLocString(nameof(Resources.Utilities_R1E_Header));
    public override GenericIconKind Icon => GenericIconKind.Utilities_Ray1Editor;

    public string HomePageURL => "https://raym.app/ray1editor/";

    public void OpenHomePage()
    {
        Services.App.OpenUrl(HomePageURL);
    }
}