﻿namespace RayCarrot.RCP.Metro.Pages.Utilities;

/// <summary>
/// View model for the utilities page
/// </summary>
public class UtilitiesPageViewModel : BasePageViewModel, IDisposable
{
    #region Constructor

    public UtilitiesPageViewModel(AppViewModel app) : base(app)
    {
        BinaryToolViewModels = new ObservableCollection<UtilityViewModel>();
        OtherViewModels = new ObservableCollection<UtilityViewModel>();
        ExternalToolViewModels = new ObservableCollection<UtilityViewModel>();
    }

    #endregion

    #region Public Properties

    public override AppPage Page => AppPage.Utilities;

    public ObservableCollection<UtilityViewModel> BinaryToolViewModels { get; }
    public ObservableCollection<UtilityViewModel> OtherViewModels { get; }
    public ObservableCollection<UtilityViewModel> ExternalToolViewModels { get; }

    #endregion

    #region Protected Methods

    protected override Task InitializeAsync()
    {
        // Create view models
        BinaryToolViewModels.AddRange(new UtilityViewModel[]
        {
            new Utility_Archives_ViewModel(),
            new Utility_Serializers_ViewModel(),
            new Utility_Converters_ViewModel(),
            new Utility_Decoders_ViewModel(),
        });
        OtherViewModels.AddRange(new UtilityViewModel[]
        {
            new Utility_R1PasswordGenerator_ViewModel(),
        });
        ExternalToolViewModels.AddRange(new UtilityViewModel[]
        {
            new Utility_Ray1Editor_ViewModel(),
        });

        return Task.CompletedTask;
    }

    #endregion

    #region Public Methods

    public void Dispose()
    {
        BinaryToolViewModels.DisposeAll();
        OtherViewModels.DisposeAll();
        ExternalToolViewModels.DisposeAll();
    }

    #endregion
}