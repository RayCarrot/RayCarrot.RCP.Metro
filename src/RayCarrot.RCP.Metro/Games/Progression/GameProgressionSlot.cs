using System;
using System.Collections.Generic;

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
        Name = name ?? new ResourceLocString(nameof(Resources.Progression_GenericSlot), index + 1);
        Index = index;
        CollectiblesCount = collectiblesCount;
        TotalCollectiblesCount = totalCollectiblesCount;
        Percentage = collectiblesCount / (double)totalCollectiblesCount * 100;
        FilePath = filePath;
        DataItems = dataItems;
        Is100Percent = CollectiblesCount == TotalCollectiblesCount;
    }

    public GameProgressionSlot(
        LocalizedString? name, 
        int index, 
        double percentage,
        FileSystemPath filePath,
        IReadOnlyList<GameProgressionDataItem> dataItems)
    {
        Name = name ?? new ResourceLocString(Resources.Progression_GenericSlot, index + 1);
        Index = index;
        CollectiblesCount = null;
        TotalCollectiblesCount = null;
        Percentage = percentage;
        FilePath = filePath;
        DataItems = dataItems;
        Is100Percent = percentage >= 100;
    }

    #endregion

    #region Public Properties

    public LocalizedString Name { get; }
    public int Index { get; }
    public int? CollectiblesCount { get; }
    public int? TotalCollectiblesCount { get; }
    public double Percentage { get; }
    public bool Is100Percent { get; }
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
}