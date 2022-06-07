using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RayCarrot.RCP.Metro;

public class EditorDropDownFieldViewModel : EditorFieldViewModel
{
    public EditorDropDownFieldViewModel(
        LocalizedString header, LocalizedString? info, 
        Func<int> getValueAction, Action<int> setValueAction, 
        Func<IReadOnlyList<DropDownItem>> getItemsAction) : base(header, info)
    {
        GetValueAction = getValueAction;
        SetValueAction = setValueAction;
        GetItemsAction = getItemsAction;
        Items = new ObservableCollection<DropDownItem>();
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
    public ObservableCollection<DropDownItem> Items { get; }

    public override void Refresh()
    {
        IReadOnlyList<DropDownItem> newItems = GetItemsAction();

        // IDEA: Maybe improve this by only resetting the items when a flag for it gets set to avoid always getting the items collection
        if (!ReferenceEquals(newItems, _prevItems))
        {
            _prevItems = newItems;

            // Set selection to -1 to avoid clearing the collection calling SelectedItem.Set
            _selectedItem = -1;
            OnPropertyChanged(nameof(SelectedItem));

            // IDEA: Find better solution to this. We could do EnableCollectionSynchronization, but then we'd have to disable it afterwards
            App.Current.Dispatcher.Invoke(() =>
            {
                Items.Clear();
                Items.AddRange(newItems);
            });

            Logger.Debug("Recreated drop-down items for drop-down with header {0}", Header);
        }

        _selectedItem = GetValueAction();
        OnPropertyChanged(nameof(SelectedItem));
    }

    public class DropDownItem
    {
        public DropDownItem(LocalizedString header, object? data)
        {
            Header = header;
            Data = data;
        }

        public LocalizedString Header { get; }
        public object? Data { get; }
    }

    public class DropDownItem<T> : DropDownItem
    {
        public DropDownItem(string header, T? data) : base(header, data) { }

        public new T? Data => (T?)base.Data;
    }
}