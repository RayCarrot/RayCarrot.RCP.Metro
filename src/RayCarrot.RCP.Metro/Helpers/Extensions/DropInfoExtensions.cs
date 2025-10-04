using GongSolutions.Wpf.DragDrop;

namespace RayCarrot.RCP.Metro;

public static class DropInfoExtensions
{
    // NOTE: Only works if source and destination are the same collections! Also doesn't take filtering into account.
    public static bool IsSourceSameAsDestination(this IDropInfo dropInfo)
    {
        int insertIndex = dropInfo.UnfilteredInsertIndex;
        if (insertIndex > dropInfo.DragInfo.SourceIndex)
            insertIndex--;

        return dropInfo.DragInfo.SourceIndex == insertIndex;
    }
}