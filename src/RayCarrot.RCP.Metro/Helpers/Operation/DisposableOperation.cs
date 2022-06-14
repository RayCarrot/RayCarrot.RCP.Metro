using System;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The disposable wrapper
/// </summary>
public sealed class DisposableOperation : IDisposable
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="disposeAction">The action to run after running the operation</param>
    /// <param name="progressUpdatedAction">The action to run when the progress has updated</param>
    /// <param name="textUpdatedAction">The action to run when the text has updated</param>
    /// <param name="disposableLock">The disposable lock</param>
    public DisposableOperation(Action disposeAction, Action<Progress>? progressUpdatedAction, Action<string> textUpdatedAction, IDisposable disposableLock)
    {
        DisposeAction = disposeAction;
        ProgressUpdatedAction = progressUpdatedAction;
        TextUpdatedAction = textUpdatedAction;
        DisposableLock = disposableLock;
    }

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
    private IDisposable DisposableLock { get; }

    public Progress Progress { get; private set; }

    public void SetText(string text)
    {
        TextUpdatedAction(text);
    }

    public void SetProgress(Progress p)
    {
        Progress = p;
        ProgressUpdatedAction?.Invoke(p);
    }

    public void Dispose()
    {
        DisposeAction.Invoke();
        DisposableLock.Dispose();
    }
}