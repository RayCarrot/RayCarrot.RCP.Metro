using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Pages.Settings.Sections;

public class LanguageSettingsSectionViewModel : SettingsSectionViewModel
{
    public LanguageSettingsSectionViewModel(AppUserData data, AppViewModel app) : base(data)
    {
        App = app;

        ContributeLocalizationCommand = new RelayCommand(ContributeLocalization);
    }

    public ICommand ContributeLocalizationCommand { get; }

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.Settings_Language));
    public override GenericIconKind Icon => GenericIconKind.Settings_Language;

    public AppViewModel App { get; }

    /// <summary>
    /// The current culture info
    /// </summary>
    public CultureInfo? CurrentCultureInfo
    {
        get => new(Data.App_CurrentCulture);
        set
        {
            if (value == null)
                return;

            Data.App_CurrentCulture = value.Name;
        }
    }

    public bool ShowIncompleteTranslations
    {
        get => Data.App_ShowIncompleteTranslations;
        set
        {
            Data.App_ShowIncompleteTranslations = value;
            Refresh();
        }
    }

    public override void Refresh()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            LocalizationManager.RefreshLanguages(Data.App_ShowIncompleteTranslations);
            OnPropertyChanged(nameof(CurrentCultureInfo));
        });
    }

    /// <summary>
    /// Opens the URL for contributing to localizing the program
    /// </summary>
    public void ContributeLocalization()
    {
        App.OpenUrl(AppURLs.TranslationUrl);
    }
}