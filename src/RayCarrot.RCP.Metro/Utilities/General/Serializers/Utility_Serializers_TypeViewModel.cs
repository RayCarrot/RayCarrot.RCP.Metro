using System;
using System.Collections.ObjectModel;
using System.Linq;
using BinarySerializer;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

public abstract class Utility_Serializers_TypeViewModel : BaseRCPViewModel, IDisposable
{
    protected Utility_Serializers_TypeViewModel(Type type, LocalizedString name, FileExtension fileExtension, ObservableCollection<Utility_Serializers_TypeModeViewModel> modes)
    {
        Type = type;
        Name = name;
        FileExtension = fileExtension;
        Modes = modes;

        if (!Modes.Any())
            throw new Exception("At least one mode has to be specified");

        SelectedMode = Modes.First();
    }

    public Type Type { get; }
    public LocalizedString Name { get; }
    public FileExtension FileExtension { get; }

    public ObservableCollection<Utility_Serializers_TypeModeViewModel> Modes { get; }
    public Utility_Serializers_TypeModeViewModel SelectedMode { get; set; }

    public abstract object? Deserialize(Context context, string fileName);
    public abstract void Serialize(Context context, string fileName, object obj);

    public void Dispose()
    {
        Name.Dispose();
        Modes.DisposeAll();
    }
}

public class Serializers_TypeViewModel<T> : Utility_Serializers_TypeViewModel
    where T : BinarySerializable, new()
{
    public Serializers_TypeViewModel(LocalizedString name, FileExtension fileExtension, ObservableCollection<Utility_Serializers_TypeModeViewModel> modes) : base(typeof(T), name, fileExtension, modes)
    { }

    public override object? Deserialize(Context context, string fileName)
    {
        return context.ReadFileData<T>(fileName, SelectedMode.Data.Encoder, SelectedMode.Data.Endian);
    }

    public override void Serialize(Context context, string fileName, object obj)
    {
        context.WriteFileData<T>(fileName, (T)obj, SelectedMode.Data.Encoder, SelectedMode.Data.Endian);
    }
}