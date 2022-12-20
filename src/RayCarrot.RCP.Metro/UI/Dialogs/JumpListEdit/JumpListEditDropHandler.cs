using System.Windows;
using System.Windows.Controls;
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;

namespace RayCarrot.RCP.Metro;

public class JumpListEditDropHandler : DefaultDropHandler
{
    public JumpListEditViewModel? ViewModel { get; set; }

    public override void DragOver(IDropInfo dropInfo)
    {
        if (ViewModel == null)
            return;

        // Verify the types are correct
        if (dropInfo.TargetCollection is not ObservableCollection<JumpListItemViewModel> destination)
            return;

        if (ViewModel.AutoSort || destination == ViewModel.NotIncluded)
        {
            dropInfo.DropTargetAdorner = typeof(DropTargetHighlightAdorner);
            dropInfo.Effects = DragDropEffects.Move;
        }
        else
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            dropInfo.Effects = DragDropEffects.Move;
        }
    }

    public override void Drop(IDropInfo dropInfo)
    {
        if (ViewModel == null || dropInfo.DragInfo == null)
            return;

        // Verify the types are correct
        if (dropInfo.TargetCollection is not ObservableCollection<JumpListItemViewModel> destination ||
            dropInfo.DragInfo.SourceCollection is not ObservableCollection<JumpListItemViewModel> source ||
            dropInfo.DragInfo.SourceItem is not JumpListItemViewModel sourceItem)
            return;

        // Get the index to insert at
        int insertIndex;
        if (ViewModel.AutoSort || destination == ViewModel.NotIncluded)
        {
            // Get a sorted index
            insertIndex = destination.SortedBinarySearch(sourceItem);
            if (insertIndex < 0)
                insertIndex = ~insertIndex;
        }
        else
        {
            // Get the current insert index
            insertIndex = GetInsertIndex(dropInfo);
        }

        // Check if the collection is the same
        if (destination == source)
        {
            int index = destination.IndexOf(sourceItem);
            
            if (index != -1)
            {
                if (insertIndex > index)
                    insertIndex--;

                // Move the item in the collection
                Move(destination, index, insertIndex);
            }
        }
        else
        {
            // Remove from the source collection
            source.Remove(sourceItem);

            // Insert to the destination collection
            destination.Insert(insertIndex, sourceItem);
        }

        if (dropInfo.VisualTarget is not ItemsControl itemsControl)
            return;

        // Select the item in the new collection
        itemsControl.SetItemSelected(sourceItem, true);
    }
}