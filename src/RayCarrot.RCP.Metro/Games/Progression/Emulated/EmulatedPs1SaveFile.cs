using BinarySerializer.PlayStation.PS1.MemoryCard;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

public class EmulatedPs1SaveFile : EmulatedSaveFile
{
    public EmulatedPs1SaveFile(FileSystemPath filePath) : base(filePath) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override async Task<EmulatedSave[]> GetSavesAsync(GameInstallation gameInstallation)
    {
        RCPContext context = new(FilePath.Parent);
        gameInstallation.GetComponents<InitializeContextComponent>().InvokeAll(context);

        MemoryCard memoryCard;

        try
        {
            using (context)
                memoryCard = await context.ReadRequiredFileDataAsync<MemoryCard>(FilePath.Name, removeFileWhenComplete: false, recreateOnWrite: false);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Reading memory card");
            return Array.Empty<EmulatedSave>();
        }

        if (gameInstallation.GameDescriptor.Structure.GetLayout(gameInstallation) is not Ps1DiscProgramLayout layout)
        {
            Logger.Warn("No matching layout found for game");
            return Array.Empty<EmulatedSave>();
        }

        List<EmulatedSave> saves = new();
        for (int i = 0; i < memoryCard.HeaderBlock.Directories.Length; i++)
        {
            DirectoryFrame dir = memoryCard.HeaderBlock.Directories[i];
            if (dir.Usability == BlockUsability.PartiallyUsed &&
                dir.Usage == BlockUsage.NoLink &&
                dir.CountryCode == layout.MemoryCardCountryCode &&
                dir.ProductCode == layout.MemoryCardProductCode)
            {
                saves.Add(new EmulatedPs1Save(this, context, memoryCard, i + 1, dir.Identifier));
            }
        }

        return saves.ToArray();
    }
}