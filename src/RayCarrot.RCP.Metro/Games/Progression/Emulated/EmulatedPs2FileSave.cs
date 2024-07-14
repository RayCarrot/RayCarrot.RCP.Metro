using System.IO;
using BinarySerializer;
using BinarySerializer.PlayStation.PS2.MemoryCard;

namespace RayCarrot.RCP.Metro;

public class EmulatedPs2FileSave : EmulatedPs2Save
{
    public EmulatedPs2FileSave(EmulatedSaveFile file, Context context, MemoryCard memoryCard, DirectoryEntry directoryEntry, string primaryFileName) : base(file, context)
    {
        MemoryCard = memoryCard;
        DirectoryEntry = directoryEntry;
        PrimaryFileName = primaryFileName;
    }

    public MemoryCard MemoryCard { get; }
    public DirectoryEntry DirectoryEntry { get; }
    public string PrimaryFileName { get; }

    public override bool CanWrite => false;

    public DirectoryEntry GetFileEntry(string fileName)
    {
        foreach (DirectoryEntry entry in DirectoryEntry.SubDirectories)
        {
            if (entry.Name == fileName && (entry.DirectoryFlags & DirectoryFlags.File) != 0 && (entry.DirectoryFlags & DirectoryFlags.Exists) != 0)
                return entry;
        }

        throw new FileNotFoundException($"Could not find {fileName} in memory card directory");
    }

    public override async Task<T> ReadAsync<T>() => await ReadAsync<T>(PrimaryFileName);

    public override Task<T> ReadAsync<T>(string fileName)
    {
        DirectoryEntry fileEntry = GetFileEntry(fileName);

        using (Context)
        {
            byte[] fileData = fileEntry.ReadFile(Context.Deserializer);
            using MemoryStream fileDataStream = new(fileData);
            
            return Task.FromResult(Context.ReadStreamData<T>(fileDataStream));
        }
    }

    public override Task WriteAsync<T>(T obj)
    {
        throw new NotImplementedException();
    }
}