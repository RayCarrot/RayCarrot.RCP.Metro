namespace RayCarrot.RCP.Metro;

public class ObservableActionItemsCollection : ObservableCollection<ActionItemViewModel>
{
    public void AddGroup(ActionItemViewModel item)
    {
        Add(item);
        Add(new SeparatorItemViewModel());
    }

    public void AddGroup(IEnumerable<ActionItemViewModel> items)
    {
        int count = Count;

        foreach (ActionItemViewModel item in items)
            Add(item);

        // Add a separator if we added some items
        if (count != Count)
            Add(new SeparatorItemViewModel());
    }
}