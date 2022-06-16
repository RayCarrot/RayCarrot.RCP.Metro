using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RayCarrot.RCP.Metro;

public class GroupedEditorDropDownFieldViewModel : EditorFieldViewModel
{
    public GroupedEditorDropDownFieldViewModel(
        LocalizedString header, LocalizedString? info, 
        Func<int> getValueAction, Action<int> setValueAction, 
        Func<IReadOnlyList<DropDownItem>> getItemsAction) : base(header, info)
    {
        GetValueAction = getValueAction;
        SetValueAction = setValueAction;
        GetItemsAction = getItemsAction;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private int _selectedItem;
    private IReadOnlyList<DropDownItem>? _prevItems;

    protected Func<int> GetValueAction { get; }
    protected Action<int> SetValueAction { get; }
    protected Func<IReadOnlyList<DropDownItem>> GetItemsAction { get; }

    public int SelectedItem
    {
        get => _selectedItem;
        set
        {
            _selectedItem = value;
            SetValueAction(value);
        }
    }
    public ObservableCollection<DropDownItem>? Items { get; set; }

    public override void Refresh()
    {
        IReadOnlyList<DropDownItem> newItems = GetItemsAction();

        if (!ReferenceEquals(newItems, _prevItems))
        {
            _prevItems = newItems;

            // Set selection to -1 to avoid clearing the collection calling SelectedItem.Set
            _selectedItem = -1;
            OnPropertyChanged(nameof(SelectedItem));

            Items = new ObservableCollection<DropDownItem>(newItems);

            LocalizedString? groupHeader = null;

            foreach (DropDownItem item in Items)
            {
                if (item.IsGroupHeader)
                    groupHeader = item.Header;

                item.GroupHeader = groupHeader;
            }

            Logger.Debug("Recreated drop-down items for drop-down with header {0}", Header);
        }

        _selectedItem = GetValueAction();
        OnPropertyChanged(nameof(SelectedItem));
    }

    public class DropDownItem
    {
        public DropDownItem(LocalizedString header, object? data, bool isGroupHeader = false)
        {
            Header = header;
            Data = data;
            IsGroupHeader = isGroupHeader;
        }

        public LocalizedString Header { get; }
        public object? Data { get; }
        public bool IsGroupHeader { get; }
        public LocalizedString? GroupHeader { get; set; }
    }

    public class DropDownItem<T> : DropDownItem
    {
        public DropDownItem(LocalizedString header, T? data, bool isGroupHeader = false) : base(header, data, isGroupHeader) { }

        public new T? Data => (T?)base.Data;
    }
}