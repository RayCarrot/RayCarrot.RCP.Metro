using System;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A disposable wrapper with support for running actions before and after the operation is finished
/// </summary>
public sealed class Operation
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="startAction">The action to run before running the operation</param>
    /// <param name="disposeAction">The action to run after running the operation</param>
    /// <param name="textUpdatedAction">The action to run when the text has updated</param>
    /// <param name="progressUpdatedAction">The optional action to run when the progress has updated</param>
    public Operation(Action<string> startAction, Action disposeAction, Action<string> textUpdatedAction, Action<Progress>? progressUpdatedAction = null)
    {
        StartAction = startAction;
        DisposeAction = disposeAction;
        TextUpdatedAction = textUpdatedAction;
        ProgressUpdatedAction = progressUpdatedAction;
        DisposableLock = new AsyncLock();
    }

    /// <summary>
    /// Runs the operation
    /// </summary>
    /// <returns>The disposable wrapper</returns>
    public async Task<DisposableOperation> RunAsync(string? displayStatus = null)
    {
        // Await the lock and get the disposable
        IDisposable d = await DisposableLock.LockAsync();

        try
        {
            // Run start action
            StartAction.Invoke(displayStatus ?? String.Empty);
        }
        catch
        {
            d.Dispose();
            throw;
        }

        // Create the disposable action
        return new DisposableOperation(DisposeAction, ProgressUpdatedAction, TextUpdatedAction, d);
    }

    /// <summary>
    /// The action to run before running the operation
    /// </summary>
    private Action<string> StartAction { get; }

    /// <summary>
    /// The action to run after running the operation
    /// </summary>
    private Action DisposeAction { get; }

    /// <summary>
    /// The action to run when the text has updated
    /// </summary>
    private Action<string> TextUpdatedAction { get; }

    /// <summary>
    /// The action to run when the progress has updated
    /// </summary>
    private Action<Progress>? ProgressUpdatedAction { get; }

    /// <summary>
    /// The disposable lock
    /// </summary>
    private AsyncLock DisposableLock { get; }
}