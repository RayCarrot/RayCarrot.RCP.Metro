using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RayCarrot.RCP.Metro;

public class Mod_GameVersionViewModel<T> : BaseViewModel
{
    public Mod_GameVersionViewModel(LocalizedString displayName, Func<Dictionary<string, long>> getOffsetsFunc, T data, IEnumerable<Mod_EmulatorViewModel> emulators)
    {
        DisplayName = displayName;
        GetOffsetsFunc = getOffsetsFunc;
        Data = data;
        Emulators = new ObservableCollection<Mod_EmulatorViewModel>(emulators);
        SelectedEmulator = Emulators.First();
    }

    public LocalizedString DisplayName { get; }
    public Func<Dictionary<string, long>> GetOffsetsFunc { get; }
    public T Data { get; }

    public ObservableCollection<Mod_EmulatorViewModel> Emulators { get; }
    public Mod_EmulatorViewModel SelectedEmulator { get; set; }
}