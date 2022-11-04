using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the utilities page
/// </summary>
public class Page_Utilities_ViewModel : BasePageViewModel, IDisposable
{
    #region Constructor

    public Page_Utilities_ViewModel(AppViewModel app) : base(app)
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
        BinaryToolViewModels.AddRange(new[]
        {
            new UtilityViewModel(new Utility_Archives()),
            new UtilityViewModel(new Utility_PatchCreator()),
            new UtilityViewModel(new Utility_Serializers()),
            new UtilityViewModel(new Utility_Converters()),
            new UtilityViewModel(new Utility_Decoders()),
        });
        OtherViewModels.AddRange(new[]
        {
            new UtilityViewModel(new Utility_R1PasswordGenerator()),
        });
        ExternalToolViewModels.AddRange(new[]
        {
            new UtilityViewModel(new Utility_Ray1Editor()),
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