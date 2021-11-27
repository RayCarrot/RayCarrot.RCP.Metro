#nullable disable
using System;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A button mapping key item view model
/// </summary>
public class ButtonMapperKeyItemViewModel : BaseRCPViewModel, IDisposable
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="actionName">The name of the action the key represents</param>
    /// <param name="originalKey">The original key for the action</param>
    /// <param name="keyChangedAction">Action to invoke when the key is changed</param>
    public ButtonMapperKeyItemViewModel(LocalizedString actionName, Key originalKey, Action keyChangedAction = null)
    {
        ActionName = actionName;
        OriginalKey = _newKey = originalKey;
        KeyChangedAction = keyChangedAction;

        ResetCommand = new RelayCommand(Reset);
    }

    #endregion

    #region Private Fields

    private Key _newKey;

    #endregion

    #region Private Properties

    /// <summary>
    /// Action to invoke when the key is changed
    /// </summary>
    private Action KeyChangedAction { get; }

    #endregion

    #region Public Properties

    /// <summary>
    /// The name of the action the key represents
    /// </summary>
    public LocalizedString ActionName { get; }

    /// <summary>
    /// The original key for the action
    /// </summary>
    public Key OriginalKey { get; }

    /// <summary>
    /// The new key for the action
    /// </summary>
    public Key NewKey
    {
        get => _newKey;
        set
        {
            _newKey = value;
            KeyChangedAction?.Invoke();
        }
    }

    #endregion

    #region Commands

    public ICommand ResetCommand { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Resets the new key to the original key
    /// </summary>
    public void Reset()
    {
        NewKey = OriginalKey;
    }

    /// <summary>
    /// Sets the initial value for the new key
    /// </summary>
    /// <param name="newKey">The new key for the action</param>
    public void SetInitialNewKey(Key newKey)
    {
        _newKey = newKey;
        OnPropertyChanged(nameof(NewKey));
    }

    public void Dispose()
    {
        ActionName?.Dispose();
    }

    #endregion
}

public class ButtonMappingKeyItemViewModel<T> : ButtonMapperKeyItemViewModel
{
    public ButtonMappingKeyItemViewModel(LocalizedString actionName, Key originalKey, T keyObj, Action keyChangedAction = null) : base(actionName, originalKey, keyChangedAction)
    {
        KeyObj = keyObj;
    }

    public T KeyObj { get; }
}