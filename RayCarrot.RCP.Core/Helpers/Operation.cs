using System;
using Nito.AsyncEx;

namespace RayCarrot.RCP.Core
{
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
        /// <param name="lockOperation">Indicates if the operation should be locked</param>
        public Operation(Action startAction, Action disposeAction, bool lockOperation)
        {
            StartAction = startAction;
            DisposeAction = disposeAction;
            LockOperation = lockOperation;
        }

        /// <summary>
        /// Runs the operation
        /// </summary>
        /// <returns>The disposable wrapper</returns>
        public DisposableAction Run()
        {
            // Run start action
            StartAction?.Invoke();

            // Create the disposable action
            return new DisposableAction(DisposeAction, LockOperation);
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
        /// Indicates if the operation should be locked
        /// </summary>
        private bool LockOperation { get; }

        /// <summary>
        /// The disposable wrapper
        /// </summary>
        public sealed class DisposableAction : IDisposable
        {
            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="disposeAction">The action to run after running the operation</param>
            /// <param name="lockOperation">Indicates if the operation should be locked</param>
            public DisposableAction(Action disposeAction, bool lockOperation)
            {
                DisposeAction = disposeAction;

                if (lockOperation)
                    DisposableLock = new AsyncLock().Lock();
            }

            /// <summary>
            /// The disposable lock
            /// </summary>
            private IDisposable DisposableLock { get; }

            /// <summary>
            /// The action to run after running the operation
            /// </summary>
            private Action DisposeAction { get; }

            public void Dispose()
            {
                DisposeAction?.Invoke();
                DisposableLock?.Dispose();
            }
        }
    }
}