using BinarySerializer;
using BinarySerializer.Nintendo.GCN;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

public class EmulatedGameCubeSaveFile : EmulatedSaveFile
{
    public EmulatedGameCubeSaveFile(FileSystemPath memoryCardPath) : base(memoryCardPath) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override async Task<EmulatedSave[]> GetSavesAsync(GameInstallation gameInstallation)
    {
        if (gameInstallation.GameDescriptor.Structure.GetLayout(gameInstallation) is not GameCubeProgramLayout layout)
        {
            Logger.Warn("No matching layout found for game");
            return Array.Empty<EmulatedSave>();
        }

        if (FilePath.FileExtension == new FileExtension(".gci"))
        {
            RCPContext context = new(FilePath.Parent);
            context.Initialize(gameInstallation);

            MemoryCardFileHeader header;
            try
            {
                using (context)
                    header = await context.ReadRequiredFileDataAsync<MemoryCardFileHeader>(FilePath.Name, endian: Endian.Big, removeFileWhenComplete: false, recreateOnWrite: false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Reading memory card file");
                return Array.Empty<EmulatedSave>();
            }

            if (header.GameCode == layout.GameCode && header.MakerCode == layout.MakerCode)
            {
                return new EmulatedSave[]
                {
                    new EmulatedGameCubeSave(this, context, header, header.FileName)
                };
            }

            return Array.Empty<EmulatedSave>();
        }
        else
        {
            // TODO: Implement reading raw memory card file
            return Array.Empty<EmulatedSave>();
        }
    }
}