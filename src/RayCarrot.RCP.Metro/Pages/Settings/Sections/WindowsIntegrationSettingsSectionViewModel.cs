using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Pages.Settings.Sections;

public class WindowsIntegrationSettingsSectionViewModel : SettingsSectionViewModel
{
    #region Constructor

    public WindowsIntegrationSettingsSectionViewModel(AppUserData data, IMessageUIManager messageUi) : base(data)
    {
        FileAssociations = new ObservableCollection<FileAssociationViewModel>(FileLaunchHandler.GetHandlers().
            Where(x => x.FileAssociationInfo != null).
            Select(x => new FileAssociationViewModel(Data, messageUi, x)));
        UriAssociations = new ObservableCollection<UriAssociationViewModel>(UriLaunchHandler.GetHandlers().
            Select(x => new UriAssociationViewModel(Data, messageUi, x)));
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.Settings_WindowsIntegration_Header));
    public override GenericIconKind Icon => GenericIconKind.Settings_WindowsIntegration;

    public ObservableCollection<FileAssociationViewModel> FileAssociations { get; }
    public ObservableCollection<UriAssociationViewModel> UriAssociations { get; }

    #endregion

    #region Public Methods

    public override void Refresh()
    {
        foreach (FileAssociationViewModel fileAssociation in FileAssociations)
            fileAssociation.Refresh();

        foreach (UriAssociationViewModel uriAssociation in UriAssociations)
            uriAssociation.Refresh();
    }

    #endregion

    #region Classes

    public abstract class AssociationViewModel<T> : BaseViewModel
        where T : LaunchArgHandler
    {
        protected AssociationViewModel(T launchHandler)
        {
            LaunchHandler = launchHandler;
            Name = launchHandler.DisplayName;

            UpdateAssociationCommand = new AsyncRelayCommand(UpdateAssociationAsync);
        }

        public ICommand UpdateAssociationCommand { get; }

        public T LaunchHandler { get; }
        public LocalizedString Name { get; }
        public bool CanAssociate { get; set; }
        public bool IsAssociated { get; set; }

        public abstract Task UpdateAssociationAsync();
        public abstract void Refresh();
    }

    public class FileAssociationViewModel : AssociationViewModel<FileLaunchHandler>
    {
        public FileAssociationViewModel(AppUserData data, IMessageUIManager messageUi, FileLaunchHandler launchHandler) : base(launchHandler)
        {
            Data = data;
            MessageUI = messageUi;
        }

        private AppUserData Data { get; }
        private IMessageUIManager MessageUI { get; }

        public override async Task UpdateAssociationAsync()
        {
            try
            {
                LaunchHandler.AssociateWithFileType(Data.App_ApplicationPath, IsAssociated);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Setting file type association");

                await MessageUI.DisplayExceptionMessageAsync(ex, Resources.Settings_WindowsIntegration_UpdateFileAssociationsError);
            }
        }

        public override void Refresh()
        {
            bool? isAssociatedWithFileType = LaunchHandler.IsAssociatedWithFileType();

            CanAssociate = isAssociatedWithFileType != null;
            IsAssociated = isAssociatedWithFileType ?? false;
        }
    }

    public class UriAssociationViewModel : AssociationViewModel<UriLaunchHandler>
    {
        public UriAssociationViewModel(AppUserData data, IMessageUIManager messageUi, UriLaunchHandler launchHandler) : base(launchHandler)
        {
            Data = data;
            MessageUI = messageUi;

        }

        private AppUserData Data { get; }
        private IMessageUIManager MessageUI { get; }

        public override async Task UpdateAssociationAsync()
        {
            try
            {
                LaunchHandler.AssociateWithUriProtocol(Data.App_ApplicationPath, IsAssociated);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Setting uri protocol association");

                await MessageUI.DisplayExceptionMessageAsync(ex, Resources.Settings_WindowsIntegration_UpdateUriAssociationsError);
            }
        }

        public override void Refresh()
        {
            bool? isAssociatedWithFileType = LaunchHandler.IsAssociatedWithUriProtocol();

            CanAssociate = isAssociatedWithFileType != null;
            IsAssociated = isAssociatedWithFileType ?? false;
        }
    }

    #endregion
}