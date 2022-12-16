#nullable disable
using System.Diagnostics;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Contains progress for an ongoing operation
/// </summary>
[DebuggerDisplay("{" + nameof(Percentage_100) + "}%")]
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
        Min = 0;
        Max = max;
    }

    /// <summary>
    /// Creates a new progress
    /// </summary>
    /// <param name="current">The current value</param>
    /// <param name="min">The minimum value</param>
    /// <param name="max">The maximum value</param>
    public Progress(double current, double min, double max)
    {
        Current = current;
        Min = min;
        Max = max;
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
    /// The progress percentage of the operation with a range of 0-100
    /// </summary>
    public double Percentage_100 => Percentage * 100;

    /// <summary>
    /// The progress percentage of the operation with a range of 0-1. If min and max
    /// are the same then 1 is returned to mark this as complete.
    /// </summary>
    public double Percentage
    {
        get
        {
            double range = Max - Min;

            // Avoid the percentage being NaN by defaulting to 100% if 0. This
            // is going to make the most sense in most cases as a progress value
            // with a max of 0 should always be seen as "finished".
            if (range == 0)
                return 1;

            return Current / range;
        }
    }

    #endregion

    #region Operators

    /// <summary>
    /// Increments the current progress
    /// </summary>
    /// <param name="progress">The progress to increment</param>
    /// <returns>The new progress</returns>
    public static Progress operator ++(Progress progress) => 
        new Progress(progress.Current + 1, progress.Min, progress.Max);

    /// <summary>
    /// Decrements the current progress
    /// </summary>
    /// <param name="progress">The progress to decrement</param>
    /// <returns>The new progress</returns>
    public static Progress operator --(Progress progress) => 
        new Progress(progress.Current - 1, progress.Min, progress.Max);

    public static Progress operator +(Progress progress, double value) =>
        new Progress(progress.Current + value, progress.Min, progress.Max);

    public static Progress operator -(Progress progress, double value) =>
        new Progress(progress.Current - value, progress.Min, progress.Max);

    #endregion

    #region Public Methods

    public Progress Add(Progress progress, double length) => 
        new Progress(Current + progress.Percentage * length, Min, Max);

    public Progress Completed() => new Progress(Max, Min, Max);

    #endregion
}