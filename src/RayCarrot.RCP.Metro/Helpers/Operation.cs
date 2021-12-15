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
    public Operation(Action startAction, Action disposeAction)
    {
        StartAction = startAction;
        DisposeAction = disposeAction;
        DisposableLock = new AsyncLock();
    }

    /// <summary>
    /// Runs the operation
    /// </summary>
    /// <returns>The disposable wrapper</returns>
    public async Task<IDisposable> RunAsync()
    {
        // Await the lock and get the disposable
        IDisposable d = await DisposableLock.LockAsync();

        try
        {
            // Run start action
            StartAction.Invoke();
        }
        catch
        {
            d.Dispose();
            throw;
        }

        // Create the disposable action
        return new DisposableAction(DisposeAction, d);
    }

    /// <summary>
    /// The action to run before running the operation
    /// </summary>
    private Action StartAction { get; }

    /// <summary>
    /// The action to run after running the operation
    /// </summary>
    private Action DisposeAction { get; }

    /// <summary>
    /// The disposable lock
    /// </summary>
    private AsyncLock DisposableLock { get; }

    /// <summary>
    /// The disposable wrapper
    /// </summary>
    public sealed class DisposableAction : IDisposable
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="disposeAction">The action to run after running the operation</param>
        /// <param name="disposableLock">The disposable lock</param>
        public DisposableAction(Action disposeAction, IDisposable disposableLock)
        {
            DisposeAction = disposeAction;
            DisposableLock = disposableLock;
        }

        /// <summary>
        /// The action to run after running the operation
        /// </summary>
        private Action DisposeAction { get; }

        /// <summary>
        /// The disposable lock
        /// </summary>
        private IDisposable DisposableLock { get; }

        public void Dispose()
        {
            DisposeAction.Invoke();
            DisposableLock.Dispose();
        }
    }
}