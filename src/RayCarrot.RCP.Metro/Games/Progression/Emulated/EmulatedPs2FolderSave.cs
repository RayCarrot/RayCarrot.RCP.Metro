using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class EmulatedPs2FolderSave : EmulatedPs2Save
{
    public EmulatedPs2FolderSave(EmulatedSaveFile file, Context context, string gameDirectory, string primaryFileName) : base(file, context)
    {
        GameDirectory = gameDirectory;
        PrimaryFileName = primaryFileName;
    }

    public string GameDirectory { get; }
    public string PrimaryFileName { get; }

    public override async Task<T> ReadAsync<T>()
    {
        using (Context)
            return await Context.ReadRequiredFileDataAsync<T>(@$"{GameDirectory}\{PrimaryFileName}", removeFileWhenComplete: false);
    }

    public override async Task<T> ReadAsync<T>(string fileName)
    {
        using (Context)
            return await Context.ReadRequiredFileDataAsync<T>(@$"{GameDirectory}\{fileName}", removeFileWhenComplete: false);
    }

    public override Task WriteAsync<T>(T obj)
    {
        using (Context)
            FileFactory.Write<T>(Context, @$"{GameDirectory}\{PrimaryFileName}", obj);

        return Task.CompletedTask;
    }
}