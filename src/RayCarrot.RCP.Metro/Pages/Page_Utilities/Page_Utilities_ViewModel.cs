#nullable disable
using System;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the utilities page
/// </summary>
public class Page_Utilities_ViewModel : BaseRCPViewModel, IDisposable
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public Page_Utilities_ViewModel()
    {
        // Create view models
        ArchiveExplorerViewModels = new UtilityViewModel[]
        {
            new UtilityViewModel(new Utility_ArchiveExplorer_R1()),
            new UtilityViewModel(new Utility_ArchiveExplorer_CNT()),
            new UtilityViewModel(new Utility_ArchiveExplorer_IPK()),
        };
        ConverterViewModels = new UtilityViewModel[]
        {
            new UtilityViewModel(new Utility_Converter_R1Save()),
            new UtilityViewModel(new Utility_Converter_R1Config()),
            new UtilityViewModel(new Utility_Converter_R2Save()),
            new UtilityViewModel(new Utility_Converter_R2Config()),
            new UtilityViewModel(new Utility_Converter_GF()),
            new UtilityViewModel(new Utility_Converter_RMSave()),
            new UtilityViewModel(new Utility_Converter_R3Save()),
            new UtilityViewModel(new Utility_Converter_LOC()),
            new UtilityViewModel(new Utility_Converter_ROSave()),
            new UtilityViewModel(new Utility_Converter_RJRSave()),
            new UtilityViewModel(new Utility_Converter_RLSave()),
        };
        DecoderViewModels = new UtilityViewModel[]
        {
            new UtilityViewModel(new Utility_Decoder_R1Lng()),
            new UtilityViewModel(new Utility_Decoder_R12Save()),
            new UtilityViewModel(new Utility_Decoder_TTSnaDsb()),
            new UtilityViewModel(new Utility_Decoder_R2SnaDsb()),
            new UtilityViewModel(new Utility_Decoder_R3Save()),
        };
        OtherViewModels = new UtilityViewModel[]
        {
            new UtilityViewModel(new Utility_SyncTextureInfo()),
            new UtilityViewModel(new Utility_R1PasswordGenerator()),
        };
        ExternalToolViewModels = new UtilityViewModel[]
        {
            new UtilityViewModel(new Utility_Ray1Editor()),
        };
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// View models for the Archive Explorer utilities
    /// </summary>
    public UtilityViewModel[] ArchiveExplorerViewModels { get; }

    /// <summary>
    /// View models for the converter utilities
    /// </summary>
    public UtilityViewModel[] ConverterViewModels { get; }

    /// <summary>
    /// View models for the decoder utilities
    /// </summary>
    public UtilityViewModel[] DecoderViewModels { get; }

    /// <summary>
    /// View models for the other utilities
    /// </summary>
    public UtilityViewModel[] OtherViewModels { get; }

    /// <summary>
    /// View models for the external tool utilities
    /// </summary>
    public UtilityViewModel[] ExternalToolViewModels { get; }

    #endregion

    #region Public Methods

    public void Dispose()
    {
        ArchiveExplorerViewModels?.DisposeAll();
        ConverterViewModels?.DisposeAll();
        DecoderViewModels?.DisposeAll();
        OtherViewModels?.DisposeAll();
        ExternalToolViewModels?.DisposeAll();
    }

    #endregion
}