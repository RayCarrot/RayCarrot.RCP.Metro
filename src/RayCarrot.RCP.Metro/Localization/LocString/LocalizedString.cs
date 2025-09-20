using System.Globalization;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A string wrapper which changes regenerates itself when the current culture changes
/// </summary>
public abstract class LocalizedString : BaseViewModel, IDisposable
{
    #region Constructors

    protected LocalizedString(bool refreshOnCultureChanged = true)
    {
        // Subscribe to when the culture changes
        if (refreshOnCultureChanged)
            CultureChangedWeakEventManager.AddHandler(Services.InstanceData, Data_CultureChanged);
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// The current string value
    /// </summary>
    public string Value { get; protected set; } = null!;

    #endregion

    #region Static Implicit Operators

    public static implicit operator LocalizedString(string str) => new ConstLocString(str);
    public static implicit operator string(LocalizedString str) => str.Value;

    #endregion

    #region Private Methods

    private void Data_CultureChanged(object sender, PropertyChangedEventArgs<CultureInfo> e)
    {
        RefreshValue();
    }

    #endregion

    #region Protected Methods

    protected abstract string GetValue();

    #endregion

    #region Public Methods

    public void RefreshValue()
    {
        Value = GetValue();
    }

    public override string ToString() => Value;

    public void Dispose()
    {
        CultureChangedWeakEventManager.RemoveHandler(Services.InstanceData, Data_CultureChanged);
    }

    #endregion
}