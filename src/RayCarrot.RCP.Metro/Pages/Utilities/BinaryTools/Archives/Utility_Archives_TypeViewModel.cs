using RayCarrot.RCP.Metro.Archive;

namespace RayCarrot.RCP.Metro.Pages.Utilities;

public class Utility_Archives_TypeViewModel : BaseRCPViewModel, IDisposable
{
    public Utility_Archives_TypeViewModel(LocalizedString name, FileExtension fileExtension, Func<Enum, ArchiveMode, IArchiveDataManager> getManagerFunc, EnumSelectionViewModel<Enum> modes)
    {
        Name = name;
        FileExtension = fileExtension;
        GetManagerFuncFunc = getManagerFunc;
        Modes = modes;
    }

    public LocalizedString Name { get; }
    public FileExtension FileExtension { get; }
    public Func<Enum, ArchiveMode, IArchiveDataManager> GetManagerFuncFunc { get; }

    public EnumSelectionViewModel<Enum> Modes { get; }

    public IArchiveDataManager GetManager(ArchiveMode mode) => GetManagerFuncFunc(Modes.SelectedValue, mode);

    public void Dispose()
    {
        Name.Dispose();
    }

    public enum ArchiveMode
    {
        Explorer,
        Creator,
    }
}