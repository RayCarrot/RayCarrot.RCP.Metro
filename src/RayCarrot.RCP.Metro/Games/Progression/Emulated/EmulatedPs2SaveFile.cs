using System.IO;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

public class EmulatedPs2SaveFile : EmulatedSaveFile
{
    public EmulatedPs2SaveFile(FileSystemPath memoryCardPath) : base(memoryCardPath) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private async Task<EmulatedSave[]> GetSavesFromFileAsync(RCPContext context, Ps2DiscProgramLayout layout)
    {
        // TODO-UPDATE: Implement
        return Array.Empty<EmulatedSave>();
    }

    private Task<EmulatedSave[]> GetSavesFromFolderAsync(RCPContext context, Ps2DiscProgramLayout layout)
    {
        List<EmulatedSave> saves = new();

        // Enumerate every directory starting with the country and product codes
        foreach (FileSystemPath saveDir in Directory.EnumerateDirectories(FilePath, $"{layout.MemoryCardCountryCode}{layout.MemoryCardProductCode}*", SearchOption.TopDirectoryOnly))
        {
            string name = saveDir.Name;
            saves.Add(new EmulatedPs2FolderSave(this, context, @$"{name}\{name}"));
        }

        return Task.FromResult(saves.ToArray());
    }

    public override async Task<EmulatedSave[]> GetSavesAsync(GameInstallation gameInstallation)
    {
        if (gameInstallation.GameDescriptor.Structure.GetLayout(gameInstallation) is not Ps2DiscProgramLayout layout)
        {
            Logger.Warn("No matching layout found for game");
            return Array.Empty<EmulatedSave>();
        }

        if (FilePath.FileExists)
        {
            RCPContext context = new(FilePath.Parent);
            gameInstallation.GetComponents<InitializeContextComponent>().InvokeAll(context);

            return await GetSavesFromFileAsync(context, layout);
        }
        else if (FilePath.DirectoryExists)
        {
            RCPContext context = new(FilePath);
            gameInstallation.GetComponents<InitializeContextComponent>().InvokeAll(context);

            return await GetSavesFromFolderAsync(context, layout);
        }
        else
        {
            Logger.Warn("Memory card not found");
            return Array.Empty<EmulatedSave>();
        }
    }
}