using System.ComponentModel;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A base view model that fires Property Changed events as needed
/// </summary>
public abstract class BaseViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// The event that is fired when any child property changes its value
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Call this to fire a <see cref="PropertyChanged"/> event
    /// </summary>
    /// <param name="propertyName">The name of the property which changed</param>
    public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}