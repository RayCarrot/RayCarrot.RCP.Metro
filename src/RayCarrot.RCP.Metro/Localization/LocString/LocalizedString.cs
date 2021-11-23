using RayCarrot.UI;
using System;
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
    public string Value { get; protected set; }

    #endregion

    #region Private Methods

    private void Data_CultureChanged(object sender, PropertyChangedEventArgs<CultureInfo> e)
    {
        RefreshValue();
    }

    #endregion

    #region Protected Methods

    protected abstract string GetValue();

    protected void RefreshValue()
    {
        Value = GetValue();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        return Value;
    }

    public void Dispose()
    {
        CultureChangedWeakEventManager.RemoveHandler(Services.InstanceData, Data_CultureChanged);
    }

    #endregion
}