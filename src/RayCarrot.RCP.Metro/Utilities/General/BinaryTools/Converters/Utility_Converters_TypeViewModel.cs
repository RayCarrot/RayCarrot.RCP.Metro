using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public abstract class Utility_Converters_TypeViewModel : BaseRCPViewModel, IDisposable
{
    protected Utility_Converters_TypeViewModel(LocalizedString name, ObservableCollection<Utility_SerializableTypeModeViewModel> modes)
    {
        Name = name;
        Modes = modes;

        if (!Modes.Any())
            throw new Exception("At least one mode has to be specified");

        SelectedMode = Modes.First();
    }

    public LocalizedString Name { get; }

    public ObservableCollection<Utility_SerializableTypeModeViewModel> Modes { get; }
    public Utility_SerializableTypeModeViewModel SelectedMode { get; set; }

    public abstract FileExtension SourceFileExtension { get; }
    public abstract string[] ConvertFormats { get; }

    protected T? ReadFile<T>(Context context, Endian endian, string fileName, Action<T>? onPreSerialize = null)
        where T : BinarySerializable, new()
    {
        return context.ReadFileData<T>(fileName, SelectedMode.Encoder, endian, onPreSerialize);
    }

    protected void WriteFile<T>(Context context, Endian endian, string fileName, T obj)
        where T : BinarySerializable, new()
    {
        context.WriteFileData<T>(fileName, obj, SelectedMode.Encoder, endian);
    }

    public abstract void Convert(Context context, string inputFileName, FileSystemPath outputFilePath);
    public virtual Task<object?> GetConvertBackStateAsync() => Task.FromResult<object?>(new object());
    public abstract void ConvertBack(Context context, FileSystemPath inputFilePath, string outputFileName, object state);

    public void Dispose()
    {
        Name.Dispose();
    }
}