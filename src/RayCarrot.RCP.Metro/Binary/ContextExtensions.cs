using System.IO;
using System.Threading.Tasks;
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public static class ContextExtensions
{
    public static T? ReadFileData<T>(this Context context, string fileName, IStreamEncoder? encoder = null, Endian endian = Endian.Little)
        where T : BinarySerializable, new()
    {
        PhysicalFile file = encoder == null
            ? new LinearFile(context, fileName, endian)
            : new EncodedLinearFile(context, fileName, encoder, endian);

        if (!File.Exists(file.SourcePath))
            return null;

        context.AddFile(file);

        return FileFactory.Read<T>(fileName, context);
    }

    public static Task<T?> ReadFileDataAsync<T>(this Context context, string fileName, IStreamEncoder? encoder = null, Endian endian = Endian.Little)
        where T : BinarySerializable, new()
    {
        return Task.Run(() => context.ReadFileData<T>(fileName, encoder, endian));
    }

    public static void WriteFileData<T>(this Context context, string fileName, T obj, IStreamEncoder? encoder = null, Endian endian = Endian.Little)
        where T : BinarySerializable, new()
    {
        PhysicalFile file = encoder == null
            ? new LinearFile(context, fileName, endian)
            : new EncodedLinearFile(context, fileName, encoder, endian);

        context.AddFile(file);

        FileFactory.Write(fileName, obj, context);
    }
}