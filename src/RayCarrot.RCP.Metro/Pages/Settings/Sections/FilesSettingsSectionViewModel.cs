using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro.Pages.Settings.Sections;

public class FilesSettingsSectionViewModel : SettingsSectionViewModel, IRecipient<FileEditorAssociationAdded>, IRecipient<FileEditorAssociationRemoved>
{
    public FilesSettingsSectionViewModel(
        AppUserData data, 
        AppUIManager ui, 
        IMessageUIManager messageUi, 
        AssociatedFileEditorsManager associatedFileEditorsManager,
        IMessenger messenger) : base(data)
    {
        UI = ui;
        MessageUI = messageUi;
        AssociatedFileEditorsManager = associatedFileEditorsManager;

        AssociatedPrograms = new ObservableCollectionEx<AssociatedProgramEntryViewModel>();

        messenger.RegisterAll(this);

        AddCommand = new AsyncRelayCommand(AddAsync);
    }

    private AppUIManager UI { get; }
    private IMessageUIManager MessageUI { get; }
    private AssociatedFileEditorsManager AssociatedFileEditorsManager { get; }

    public ICommand AddCommand { get; }

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.Settings_Files_Header));
    public override GenericIconKind Icon => GenericIconKind.Settings_Files;

    public ObservableCollectionEx<AssociatedProgramEntryViewModel> AssociatedPrograms { get; }

    private void SortPrograms()
    {
        AssociatedPrograms.ModifyCollection(x => x.Sort((item1, item2) => String.Compare(item1.FileExtension, item2.FileExtension, StringComparison.Ordinal)));
    }

    public async Task AddAsync()
    {
        StringInputResult stringResult = await UI.GetStringInputAsync(new StringInputViewModel()
        {
            Title = Resources.Settings_Files_FileExtInputTitle,
            HeaderText = Resources.Settings_Files_FileExtInputHeader
        });

        if (stringResult.CanceledByUser)
            return;

        string ext = stringResult.StringInput.ToLowerInvariant();

        if (AssociatedFileEditorsManager.GetFileEditorAssociations().ContainsKey(ext))
        {
            await MessageUI.DisplayMessageAsync(Resources.Settings_Files_ConflictError, MessageType.Error);
            return;
        }

        bool isBinary = ext == AssociatedFileEditorsManager.BinaryFileExtension;

        if (!isBinary && (!ext.StartsWith(".") || ext.Count(x => x == '.') > 1))
        {
            await MessageUI.DisplayMessageAsync(Resources.Settings_Files_FormatError, MessageType.Error);
            return;
        }

        ProgramSelectionResult programResult = await UI.GetProgramAsync(new ProgramSelectionViewModel
        {
            Title = Resources.Settings_Files_ProgramSelectionTitle,
            FileExtensions = isBinary ? Array.Empty<FileExtension>() : new[] { new FileExtension(ext) },
        });

        if (programResult.CanceledByUser)
            return;

        AssociatedFileEditorsManager.AddFileEditorAssociaton(ext, programResult.ProgramFilePath);
    }

    public override async void Refresh()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            AssociatedPrograms.ModifyCollection(x =>
            {
                x.Clear();
                x.AddRange(AssociatedFileEditorsManager.GetFileEditorAssociations().
                    Select(p => new AssociatedProgramEntryViewModel(UI, AssociatedFileEditorsManager, p.Key)).
                    OrderBy(p => p.FileExtension));
            });
        });

        await Task.WhenAll(AssociatedPrograms.Select(x => Task.Run(x.LoadIcon)));
    }

    public void Receive(FileEditorAssociationAdded message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            AssociatedProgramEntryViewModel vm = new(UI, AssociatedFileEditorsManager, message.FileExtension);
            vm.LoadIcon();
            AssociatedPrograms.Add(vm);
            SortPrograms();
        });
    }

    public void Receive(FileEditorAssociationRemoved message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            AssociatedPrograms.RemoveWhere(x => x.FileExtension == message.FileExtension);
            SortPrograms();
        });
    }

    public class AssociatedProgramEntryViewModel : BaseRCPViewModel
    {
        public AssociatedProgramEntryViewModel(AppUIManager ui, AssociatedFileEditorsManager associatedFileEditorsManager, string fileExtension)
        {
            UI = ui;
            AssociatedFileEditorsManager = associatedFileEditorsManager;
            FileExtension = fileExtension;

            ChangeProgramCommand = new AsyncRelayCommand(ChangeProgramAsync);
            RemoveCommand = new RelayCommand(Remove);
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private AppUIManager UI { get; }
        private AssociatedFileEditorsManager AssociatedFileEditorsManager { get; }

        public ICommand ChangeProgramCommand { get; }
        public ICommand RemoveCommand { get; }

        public string FileExtension { get; }
        public string ExeFilePath
        {
            get => AssociatedFileEditorsManager.GetFileEditorAssociaton(FileExtension) ?? String.Empty;
            set => AssociatedFileEditorsManager.UpdateFileEditorAssociaton(FileExtension, value);
        }
        public ImageSource? ExeIconImageSource { get; set; }

        public async Task ChangeProgramAsync()
        {
            ProgramSelectionResult programResult = await UI.GetProgramAsync(new ProgramSelectionViewModel
            {
                Title = Resources.Settings_Files_ChangeProgramSelectionTitle,
                ProgramFilePath = ExeFilePath,
                FileExtensions = new[] { new FileExtension(FileExtension) },
            });

            if (programResult.CanceledByUser)
                return;

            ExeFilePath = programResult.ProgramFilePath;

            LoadIcon();
        }

        public void Remove()
        {
            AssociatedFileEditorsManager.RemoveFileEditorAssociaton(FileExtension);
        }

        public void LoadIcon()
        {
            try
            {
                ExeIconImageSource = WindowsHelpers.GetIconOrThumbnail(ExeFilePath, ShellThumbnailSize.Medium).ToImageSource();
                ExeIconImageSource?.Freeze();
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Getting exe icon image source");
            }
        }
    }
}