using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public abstract class Utility_Serializers_TypeViewModel : BaseRCPViewModel, IDisposable
{
    protected Utility_Serializers_TypeViewModel(Type type, LocalizedString name, FileExtension fileExtension, Func<Context, Endian> getEndianFunc, ObservableCollection<Utility_SerializableTypeModeViewModel> modes)
    {
        Type = type;
        Name = name;
        FileExtension = fileExtension;
        GetEndianFunc = getEndianFunc;
        Modes = modes;

        if (!Modes.Any())
            throw new Exception("At least one mode has to be specified");

        SelectedMode = Modes.First();
    }

    public Type Type { get; }
    public LocalizedString Name { get; }
    public FileExtension FileExtension { get; }

    public Func<Context, Endian> GetEndianFunc { get; }

    public ObservableCollection<Utility_SerializableTypeModeViewModel> Modes { get; }
    public Utility_SerializableTypeModeViewModel SelectedMode { get; set; }

    public abstract object? Deserialize(Context context, string fileName);
    public abstract void Serialize(Context context, string fileName, object obj);

    public void Dispose()
    {
        Name.Dispose();
    }
}

public class Serializers_TypeViewModel<T> : Utility_Serializers_TypeViewModel
    where T : BinarySerializable, new()
{
    public Serializers_TypeViewModel(LocalizedString name, FileExtension fileExtension, Func<Context, Endian> getEndianFunc, ObservableCollection<Utility_SerializableTypeModeViewModel> modes) : base(typeof(T), name, fileExtension, getEndianFunc, modes)
    { }

    public override object? Deserialize(Context context, string fileName)
    {
        return context.ReadFileData<T>(fileName, SelectedMode.Encoder, GetEndianFunc(context));
    }

    public override void Serialize(Context context, string fileName, object obj)
    {
        context.WriteFileData<T>(fileName, (T)obj, SelectedMode.Encoder, GetEndianFunc(context));
    }
}