namespace RayCarrot.RCP.Metro;

public class GameProgressionSlot
{
    #region Constructors

    public GameProgressionSlot(
        LocalizedString? name, 
        int index,
        int collectiblesCount, 
        int totalCollectiblesCount,
        FileSystemPath filePath,
        IReadOnlyList<GameProgressionDataItem> dataItems)
    {
        if (name != null)
            Name = name;
        else if (index != -1)
            Name = new ResourceLocString(nameof(Resources.Progression_GenericSlot), index + 1);
        else
            Name = new ResourceLocString(nameof(Resources.Progression_GenericSingleSlot));

        Index = index;
        CollectiblesCount = collectiblesCount;
        TotalCollectiblesCount = totalCollectiblesCount;
        Percentage = collectiblesCount / (double)totalCollectiblesCount * 100;
        FilePath = filePath;
        DataItems = dataItems;

        if (CollectiblesCount >= TotalCollectiblesCount)
            State = ProgressionState.FullyComplete;
        else if (Percentage >= 50)
            State = ProgressionState.HalfWayComplete;
        else
            State = ProgressionState.NotComplete;
    }

    public GameProgressionSlot(
        LocalizedString? name, 
        int index, 
        double percentage,
        FileSystemPath filePath,
        IReadOnlyList<GameProgressionDataItem> dataItems)
    {
        if (name != null)
            Name = name;
        else if (index != -1)
            Name = new ResourceLocString(nameof(Resources.Progression_GenericSlot), index + 1);
        else
            Name = new ResourceLocString(nameof(Resources.Progression_GenericSingleSlot));

        Index = index;
        CollectiblesCount = null;
        TotalCollectiblesCount = null;
        Percentage = percentage;
        FilePath = filePath;
        DataItems = dataItems;

        if (percentage >= 100)
            State = ProgressionState.FullyComplete;
        else if (Percentage >= 50)
            State = ProgressionState.HalfWayComplete;
        else
            State = ProgressionState.NotComplete;
    }

    #endregion

    #region Public Properties

    public LocalizedString Name { get; }
    public int Index { get; }
    public int? CollectiblesCount { get; }
    public int? TotalCollectiblesCount { get; }
    public double Percentage { get; }
    public ProgressionState State { get; }
    public FileSystemPath FilePath { get; }

    public int SlotGroup { get; init; }

    public IReadOnlyList<GameProgressionDataItem> DataItems { get; }

    public virtual bool CanExport => false;
    public virtual bool CanImport => false;

    #endregion

    #region Public Methods

    public virtual void ExportSlot(FileSystemPath filePath) => 
        throw new NotSupportedException("This slot does not support exporting slots");
    public virtual void ImportSlot(FileSystemPath filePath) => 
        throw new NotSupportedException("This slot does not support importing slots");

    #endregion

    #region Enums

    public enum ProgressionState
    {
        FullyComplete,
        HalfWayComplete,
        NotComplete,
    }

    #endregion
}