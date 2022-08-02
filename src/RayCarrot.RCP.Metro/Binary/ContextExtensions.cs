using System;
using System.IO;
using System.Threading.Tasks;
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public static class ContextExtensions
{
    public static T? ReadFileData<T>(this Context context, string fileName, IStreamEncoder? encoder = null, Endian? endian = null, Action<T>? onPreSerialize = null, bool removeFileWhenComplete = true)
        where T : BinarySerializable, new()
    {
        PhysicalFile file = encoder == null
            ? new LinearFile(context, fileName, endian)
            : new EncodedLinearFile(context, fileName, encoder, endian);

        if (!File.Exists(file.SourcePath))
            return null;

        context.AddFile(file);

        try
        {
            T data = FileFactory.Read<T>(context, fileName, (_, o) => onPreSerialize?.Invoke(o));

            if (removeFileWhenComplete)
                context.RemoveFile(file);

            return data;
        }
        catch
        {
            // Always remove the file from the context if there's an exception when reading it
            context.RemoveFile(file);
            throw;
        }
    }

    public static T ReadRequiredFileData<T>(this Context context, string fileName, IStreamEncoder? encoder = null,
        Endian? endian = null, Action<T>? onPreSerialize = null, bool removeFileWhenComplete = true)
        where T : BinarySerializable, new()
    {
        return ReadFileData<T>(context, fileName, encoder, endian, onPreSerialize, removeFileWhenComplete) 
               ?? throw new FileNotFoundException($"The requested file {fileName} was not found");
    }

    public static T ReadStreamData<T>(this Context context, Stream stream, string name = "Stream", Endian? endian = null, bool leaveOpen = false, Action<T>? onPreSerialize = null)
        where T : BinarySerializable, new()
    {
        BinaryFile file = new StreamFile(context, name, stream, endian, leaveOpen: leaveOpen);

        context.AddFile(file);

        try
        {
            return FileFactory.Read<T>(context, name, (_, o) => onPreSerialize?.Invoke(o));
        }
        finally
        {
            context.RemoveFile(file);
        }
    }

    public static Task<T?> ReadFileDataAsync<T>(this Context context, string fileName, IStreamEncoder? encoder = null, Endian? endian = null, Action<T>? onPreSerialize = null, bool removeFileWhenComplete = true)
        where T : BinarySerializable, new()
    {
        return Task.Run(() => context.ReadFileData<T>(fileName, encoder, endian, onPreSerialize, removeFileWhenComplete));
    }

    public static void WriteFileData<T>(this Context context, string fileName, T obj, IStreamEncoder? encoder = null, Endian? endian = null)
        where T : BinarySerializable, new()
    {
        PhysicalFile file = encoder == null
            ? new LinearFile(context, fileName, endian)
            : new EncodedLinearFile(context, fileName, encoder, endian);

        context.AddFile(file);

        try
        {
            FileFactory.Write(context, fileName, obj);
        }
        finally
        {
            context.RemoveFile(file);
        }
    }

    public static void WriteStreamData<T>(this Context context, Stream stream, T obj, string name = "Stream", Endian? endian = null, bool leaveOpen = false)
        where T : BinarySerializable, new()
    {
        BinaryFile file = new StreamFile(context, name, stream, endian, leaveOpen: leaveOpen);

        context.AddFile(file);

        try
        {
            FileFactory.Write(context, name, obj);
        }
        finally
        {
            context.RemoveFile(file);
        }
    }
}