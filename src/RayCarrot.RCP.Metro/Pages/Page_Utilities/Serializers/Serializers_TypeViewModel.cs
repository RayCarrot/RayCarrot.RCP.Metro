using System;
using System.Collections.ObjectModel;
using System.Linq;
using BinarySerializer;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

public abstract class Serializers_TypeViewModel : BaseRCPViewModel, IDisposable
{
    protected Serializers_TypeViewModel(LocalizedString name, FileExtension[] fileExtensions, ObservableCollection<Serializers_TypeModeViewModel> modes)
    {
        Name = name;
        FileExtensions = fileExtensions;
        Modes = modes;

        if (!Modes.Any())
            throw new Exception("At least one mode has to be specified");

        SelectedMode = Modes.First();
    }

    public LocalizedString Name { get; }
    public FileExtension[] FileExtensions { get; }

    public ObservableCollection<Serializers_TypeModeViewModel> Modes { get; }
    public Serializers_TypeModeViewModel SelectedMode { get; set; }

    public abstract object? Deserialize(Context context, string fileName);

    public void Dispose()
    {
        Name.Dispose();
        Modes.DisposeAll();
    }
}

public class Serializers_TypeViewModel<T> : Serializers_TypeViewModel
    where T : BinarySerializable, new()
{
    public Serializers_TypeViewModel(LocalizedString name, FileExtension[] fileExtensions, ObservableCollection<Serializers_TypeModeViewModel> modes) : base(name, fileExtensions, modes)
    { }

    public override object? Deserialize(Context context, string fileName)
    {
        return context.ReadFileData<T>(fileName, SelectedMode.Data.Encoder, SelectedMode.Data.Endian);
    }
}