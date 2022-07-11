#nullable disable
using System.Diagnostics;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Contains progress for an ongoing operation
/// </summary>
[DebuggerDisplay("{" + nameof(Percentage) + "}%")]
public readonly struct Progress
{
    #region Constructors

    /// <summary>
    /// Creates a new progress
    /// </summary>
    /// <param name="current">The current value</param>
    public Progress(double current)
    {
        Current = current;
        Max = 100;
        Min = 0;
    }

    /// <summary>
    /// Creates a new progress
    /// </summary>
    /// <param name="current">The current value</param>
    /// <param name="max">The maximum value</param>
    public Progress(double current, double max)
    {
        Current = current;
        Max = max;
        Min = 0;
    }

    /// <summary>
    /// Creates a new progress
    /// </summary>
    /// <param name="current">The current value</param>
    /// <param name="max">The maximum value</param>
    /// <param name="min">The minimum value</param>
    public Progress(double current, double max, double min)
    {
        Current = current;
        Max = max;
        Min = min;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The maximum progress value
    /// </summary>
    public double Max { get; }

    /// <summary>
    /// The current progress value
    /// </summary>
    public double Current { get; }

    /// <summary>
    /// The minimum progress value
    /// </summary>
    public double Min { get; }

    /// <summary>
    /// The progress percentage of the operation
    /// </summary>
    public double Percentage => Current / (Max - Min) * 100; // TODO: Wouldn't 0-1 make more sense?

    #endregion

    #region Operators

    /// <summary>
    /// Increments the current progress
    /// </summary>
    /// <param name="progress">The progress to increment</param>
    /// <returns>The new progress</returns>
    public static Progress operator ++(Progress progress)
        => new Progress(progress.Current + 1, progress.Max, progress.Min);

    /// <summary>
    /// Decrements the current progress
    /// </summary>
    /// <param name="progress">The progress to decrement</param>
    /// <returns>The new progress</returns>
    public static Progress operator --(Progress progress)
        => new Progress(progress.Current - 1, progress.Max, progress.Min);

    #endregion
}