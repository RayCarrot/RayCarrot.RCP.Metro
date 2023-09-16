using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Pages.Settings.Sections;

public class WindowsIntegrationSettingsSectionViewModel : SettingsSectionViewModel
{
    #region Constructor

    public WindowsIntegrationSettingsSectionViewModel(AppUserData data, IMessageUIManager messageUi) : base(data)
    {
        MessageUI = messageUi;

        UpdatePatchFileTypeAssociationCommand = new AsyncRelayCommand(UpdatePatchFileTypeAssociationAsync);
        UpdatePatchURIProtocolAssociationCommand = new AsyncRelayCommand(UpdatePatchURIProtocolAssociationAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand UpdatePatchFileTypeAssociationCommand { get; }
    public ICommand UpdatePatchURIProtocolAssociationCommand { get; }

    #endregion

    #region Services

    private IMessageUIManager MessageUI { get; }

    #endregion

    #region Public Properties

    public override LocalizedString Header => "Windows Integration"; // TODO-UPDATE: Localize
    public override GenericIconKind Icon => GenericIconKind.Settings_WindowsIntegration;

    public bool CanAssociatePatchFileType { get; set; }
    public bool CanAssociatePatchURIProtocol { get; set; }
    public bool AssociatePatchFileType { get; set; }
    public bool AssociatePatchURIProtocol { get; set; }

    #endregion

    #region Public Methods

    // TODO-UPDATE: Rework UI to have these things be dynamic based on the available handlers
    public async Task UpdatePatchFileTypeAssociationAsync()
    {
        try
        {
            new ModFileLaunchHandler().AssociateWithFileType(Data.App_ApplicationPath, AssociatePatchFileType);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Setting mod file type association");

            // TODO-UPDATE: Update localization
            await MessageUI.DisplayExceptionMessageAsync(ex, Resources.Patcher_AssociateFileTypeError);
        }
    }

    public async Task UpdatePatchURIProtocolAssociationAsync()
    {
        try
        {
            new ModFileUriLaunchHandler().AssociateWithUriProtocol(Data.App_ApplicationPath, AssociatePatchURIProtocol);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Setting mod uri protocol association");

            // TODO-UPDATE: Update localization
            await MessageUI.DisplayExceptionMessageAsync(ex, Resources.Patcher_AssociateURIProtocolError);
        }
    }

    public override void Refresh()
    {
        bool? isAssociatedWithFileType = new ModFileLaunchHandler().IsAssociatedWithFileType();
        bool? isAssociatedWithURIProtocol = new ModFileUriLaunchHandler().IsAssociatedWithUriProtocol();

        CanAssociatePatchFileType = isAssociatedWithFileType != null;
        CanAssociatePatchURIProtocol = isAssociatedWithURIProtocol != null;

        AssociatePatchFileType = isAssociatedWithFileType ?? false;
        AssociatePatchURIProtocol = isAssociatedWithURIProtocol ?? false;
    }

    #endregion
}