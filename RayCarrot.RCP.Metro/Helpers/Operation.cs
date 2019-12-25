using System;

namespace RayCarrot.RCP.Metro
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
        public Operation(Action startAction, Action disposeAction)
        {
            StartAction = startAction;
            DisposeAction = disposeAction;
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
            return new DisposableAction(DisposeAction);
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
        /// The disposable wrapper
        /// </summary>
        public sealed class DisposableAction : IDisposable
        {
            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="disposeAction">The action to run after running the operation</param>
            public DisposableAction(Action disposeAction)
            {
                DisposeAction = disposeAction;
            }

            /// <summary>
            /// The action to run after running the operation
            /// </summary>
            private Action DisposeAction { get; }

            public void Dispose()
            {
                DisposeAction?.Invoke();
            }
        }
    }
}