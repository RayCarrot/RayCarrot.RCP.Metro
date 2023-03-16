using System.Collections.Specialized;
using System.ComponentModel;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extended version of <see cref="ObservableCollection{T}"/>
/// </summary>
/// <typeparam name="T">The item type</typeparam>
public class ObservableCollectionEx<T> : ObservableCollection<T>
{
    public ObservableCollectionEx() { }
    public ObservableCollectionEx(List<T> list) : base(list) { }
    public ObservableCollectionEx(IEnumerable<T> collection) : base(collection) { }

    private bool _notificationSupressed;
    private bool _supressNotification;

    public bool SupressNotification
    {
        get => _supressNotification;
        set
        {
            _supressNotification = value;
    
            if (!_supressNotification && _notificationSupressed)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                _notificationSupressed = false;
            }
        }
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (SupressNotification)
        {
            _notificationSupressed = true;
            return;
        }

        base.OnCollectionChanged(e);
    }

    public void ModifyCollection(Action<List<T>> action)
    {
        try
        {
            action((List<T>)Items);
        }
        finally
        {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}