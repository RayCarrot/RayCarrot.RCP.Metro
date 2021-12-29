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

    /// <summary>
    /// Default constructor
    /// </summary>
    public Page_Utilities_ViewModel()
    {
        ArchiveExplorerViewModels = new ObservableCollection<UtilityViewModel>();
        ConverterViewModels = new ObservableCollection<UtilityViewModel>();
        DecoderViewModels = new ObservableCollection<UtilityViewModel>();
        OtherViewModels = new ObservableCollection<UtilityViewModel>();
        ExternalToolViewModels = new ObservableCollection<UtilityViewModel>();
    }

    #endregion

    #region Public Properties

    public override AppPage Page => AppPage.Utilities;

    /// <summary>
    /// View models for the Archive Explorer utilities
    /// </summary>
    public ObservableCollection<UtilityViewModel> ArchiveExplorerViewModels { get; }

    public Serializers_ViewModel? SerializersViewModel { get; set; }

    /// <summary>
    /// View models for the converter utilities
    /// </summary>
    public ObservableCollection<UtilityViewModel> ConverterViewModels { get; }

    /// <summary>
    /// View models for the decoder utilities
    /// </summary>
    public ObservableCollection<UtilityViewModel> DecoderViewModels { get; }

    /// <summary>
    /// View models for the other utilities
    /// </summary>
    public ObservableCollection<UtilityViewModel> OtherViewModels { get; }

    /// <summary>
    /// View models for the external tool utilities
    /// </summary>
    public ObservableCollection<UtilityViewModel> ExternalToolViewModels { get; }

    #endregion

    #region Protected Methods

    protected override Task InitializeAsync()
    {
        // Create view models
        ArchiveExplorerViewModels.AddRange(new UtilityViewModel[]
        {
            new UtilityViewModel(new Utility_ArchiveExplorer_R1()),
            new UtilityViewModel(new Utility_ArchiveExplorer_CNT()),
            new UtilityViewModel(new Utility_ArchiveExplorer_IPK()),
        });
        SerializersViewModel = new Serializers_ViewModel();
        ConverterViewModels.AddRange(new UtilityViewModel[]
        {
            new UtilityViewModel(new Utility_Converter_GF()),
            new UtilityViewModel(new Utility_Converter_LOC()),
        });
        DecoderViewModels.AddRange(new UtilityViewModel[]
        {
            new UtilityViewModel(new Utility_Decoder_R1Lng()),
            new UtilityViewModel(new Utility_Decoder_R12Save()),
            new UtilityViewModel(new Utility_Decoder_TTSnaDsb()),
            new UtilityViewModel(new Utility_Decoder_R2SnaDsb()),
            new UtilityViewModel(new Utility_Decoder_R3Save()),
        });
        OtherViewModels.AddRange(new UtilityViewModel[]
        {
            new UtilityViewModel(new Utility_SyncTextureInfo()),
            new UtilityViewModel(new Utility_R1PasswordGenerator()),
        });
        ExternalToolViewModels.AddRange(new UtilityViewModel[]
        {
            new UtilityViewModel(new Utility_Ray1Editor()),
        });

        return Task.CompletedTask;
    }

    #endregion

    #region Public Methods

    public void Dispose()
    {
        ArchiveExplorerViewModels.DisposeAll();
        SerializersViewModel?.Dispose();
        ConverterViewModels.DisposeAll();
        DecoderViewModels.DisposeAll();
        OtherViewModels.DisposeAll();
        ExternalToolViewModels.DisposeAll();
    }

    #endregion
}