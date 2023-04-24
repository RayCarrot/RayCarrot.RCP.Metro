namespace RayCarrot.RCP.Metro;

public class EmulatedGameProgressionSlot : GameProgressionSlot
{
    public EmulatedGameProgressionSlot(
        LocalizedString? name, 
        int index, 
        int collectiblesCount, 
        int totalCollectiblesCount,
        EmulatedSave emulatedSave, 
        IReadOnlyList<GameProgressionDataItem> dataItems) 
        : base(name, index, collectiblesCount, totalCollectiblesCount, emulatedSave.File.FilePath, dataItems)
    {
        EmulatedSave = emulatedSave;
    }

    public EmulatedGameProgressionSlot(
        LocalizedString? name, 
        int index, 
        double percentage, 
        EmulatedSave emulatedSave, 
        IReadOnlyList<GameProgressionDataItem> dataItems) 
        : base(name, index, percentage, emulatedSave.File.FilePath, dataItems)
    {
        EmulatedSave = emulatedSave;
    }

    public EmulatedSave EmulatedSave { get; }
}