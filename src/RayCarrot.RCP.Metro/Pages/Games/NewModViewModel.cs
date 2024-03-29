﻿using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Pages.Games;

public class NewModViewModel : BaseViewModel
{
    public NewModViewModel(string name, DateTime modificationDate, string modUrl, bool isUpdate, IEnumerable<GameDescriptor> gameDescriptors)
    {
        Name = name;
        ModificationDate = modificationDate;
        
        if (isUpdate)
            ModificationDateDisplayText = new GeneratedLocString(() => String.Format(Resources.ModNews_UpdatedInfo, $"{modificationDate:D}"));
        else
            ModificationDateDisplayText = new GeneratedLocString(() => String.Format(Resources.ModNews_UploadedInfo, $"{modificationDate:D}"));
        
        ModUrl = modUrl;
        GameDescriptors = new ObservableCollection<GameDescriptor>(gameDescriptors);

        OpenModCommand = new RelayCommand(OpenMod);
    }

    public ICommand OpenModCommand { get; }

    public string Name { get; }
    public DateTime ModificationDate { get; }
    public LocalizedString ModificationDateDisplayText { get; }
    public string ModUrl { get; }
    public ObservableCollection<GameDescriptor> GameDescriptors { get; }

    public void OpenMod() => Services.App.OpenUrl(ModUrl);
}