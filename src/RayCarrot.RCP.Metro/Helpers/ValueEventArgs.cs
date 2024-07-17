namespace RayCarrot.RCP.Metro;

/// <summary>
/// Event arguments for a single value
/// </summary>
/// <typeparam name="T">The type of value</typeparam>
public class ValueEventArgs<T> : EventArgs
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="value">The value</param>
    public ValueEventArgs(T value)
    {
        Value = value;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// The value
    /// </summary>
    public T Value { get; }

    #endregion
}