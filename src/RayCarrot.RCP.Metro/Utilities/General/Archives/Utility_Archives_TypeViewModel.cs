using System;
using System.Collections.ObjectModel;
using System.Linq;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

public class Utility_Archives_TypeViewModel : BaseRCPViewModel, IDisposable
{
    public Utility_Archives_TypeViewModel(LocalizedString name, FileExtension fileExtension, Func<object, ArchiveMode, IArchiveDataManager> getManagerFunc, ObservableCollection<Utility_Archives_ModeViewModel> modes)
    {
        Name = name;
        FileExtension = fileExtension;
        GetManagerFuncFunc = getManagerFunc;
        Modes = modes;

        if (!Modes.Any())
            throw new Exception("At least one mode has to be specified");

        SelectedMode = Modes.First();
    }

    public LocalizedString Name { get; }
    public FileExtension FileExtension { get; }
    public Func<object, ArchiveMode, IArchiveDataManager> GetManagerFuncFunc { get; }

    public ObservableCollection<Utility_Archives_ModeViewModel> Modes { get; }
    public Utility_Archives_ModeViewModel SelectedMode { get; set; }

    public IArchiveDataManager GetManager(ArchiveMode mode) => GetManagerFuncFunc(SelectedMode.Data, mode);

    public void Dispose()
    {
        Name.Dispose();
        Modes.DisposeAll();
    }

    public enum ArchiveMode
    {
        Explorer,
        Creator,
    }
}