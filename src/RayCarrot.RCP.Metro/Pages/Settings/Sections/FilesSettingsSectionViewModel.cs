using System.Collections.Specialized;
using System.Windows;

namespace RayCarrot.RCP.Metro.Pages.Settings.Sections;

public class FilesSettingsSectionViewModel : SettingsSectionViewModel
{
    public FilesSettingsSectionViewModel(AppUserData data) : base(data)
    {
        AssociatedPrograms = new ObservableCollection<AssociatedProgramEntryViewModel>();

        // Refresh when needed
        Data.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Data.Archive_AssociatedPrograms))
                Refresh();
        };
        AssociatedPrograms.CollectionChanged += (_, e) =>
        {
            // For now you can only remove items from the UI
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (AssociatedProgramEntryViewModel item in e.OldItems)
                    Data.Archive_AssociatedPrograms.Remove(item.FileExtension);

                // Make sure the check for if the collection is empty or not updates
                OnPropertyChanged(nameof(AssociatedPrograms));
            }
        };
    }

    public override LocalizedString Header => "Files"; // TODO-UPDATE: Localize
    public override GenericIconKind Icon => GenericIconKind.Settings_Files;

    public ObservableCollection<AssociatedProgramEntryViewModel> AssociatedPrograms { get; }

    public override void Refresh()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            AssociatedPrograms.Clear();

            AssociatedPrograms.AddRange(Data.Archive_AssociatedPrograms.Select(x => new AssociatedProgramEntryViewModel(x.Key)));

            OnPropertyChanged(nameof(AssociatedPrograms));
        });
    }

    public class AssociatedProgramEntryViewModel : BaseRCPViewModel
    {
        public AssociatedProgramEntryViewModel(string fileExtension)
        {
            FileExtension = fileExtension;
        }

        public string FileExtension { get; }
        public string ExeFilePath
        {
            get => Data.Archive_AssociatedPrograms[FileExtension];
            set => Data.Archive_AssociatedPrograms[FileExtension] = value;
        }
    }
}