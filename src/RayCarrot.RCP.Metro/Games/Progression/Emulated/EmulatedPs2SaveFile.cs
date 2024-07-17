using System.IO;
using BinarySerializer.PlayStation.PS2.MemoryCard;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

public class EmulatedPs2SaveFile : EmulatedSaveFile
{
    public EmulatedPs2SaveFile(FileSystemPath memoryCardPath) : base(memoryCardPath) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private async Task<EmulatedSave[]> GetSavesFromFileAsync(RCPContext context, Ps2DiscProgramLayout layout)
    {
        MemoryCard memoryCard;

        try
        {
            using (context)
            {
                MemoryCardFile memCardFile = new(context, FilePath.Name);
                memoryCard = await context.ReadRequiredFileDataAsync<MemoryCard>(memCardFile, removeFileWhenComplete: false, recreateOnWrite: false);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Reading memory card");
            return Array.Empty<EmulatedSave>();
        }

        List<EmulatedSave> saves = new();
        
        // Get every directory starting with the country and product codes
        string namePrefix = $"{layout.CountryCode}{layout.ProductCode}";

        foreach (DirectoryEntry dir in memoryCard.RootDirectory.SubDirectories)
        {
            if ((dir.DirectoryFlags & DirectoryFlags.Directory) != 0 && 
                (dir.DirectoryFlags & DirectoryFlags.Exists) != 0 && 
                dir.Name.StartsWith(namePrefix))
            {
                string primaryFileName = dir.Name;
                saves.Add(new EmulatedPs2FileSave(this, context, memoryCard, dir, primaryFileName));
            }
        }

        return saves.ToArray();
    }

    private Task<EmulatedSave[]> GetSavesFromFolderAsync(RCPContext context, Ps2DiscProgramLayout layout)
    {
        List<EmulatedSave> saves = new();

        // Enumerate every directory starting with the country and product codes
        foreach (FileSystemPath saveDir in Directory.EnumerateDirectories(FilePath, $"{layout.CountryCode}{layout.ProductCode}*", SearchOption.TopDirectoryOnly))
        {
            string name = saveDir.Name;
            saves.Add(new EmulatedPs2FolderSave(this, context, name, name));
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