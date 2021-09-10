using System;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Extensions methods for <see cref="AsyncEventHandler{TEventArgs}"/>
    /// </summary>
    public static class AsyncEventHandlertExtensions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Raises the event async
        /// </summary>
        /// <typeparam name="TEventArgs">The type of event args</typeparam>
        /// <param name="handlers">The handlers to raise</param>
        /// <param name="sender">The sender</param>
        /// <param name="eventArgs">The event args</param>
        /// <returns>The task</returns>
        public static async Task RaiseAsync<TEventArgs>(this AsyncEventHandler<TEventArgs> handlers, object sender, TEventArgs eventArgs)
            where TEventArgs : EventArgs
        {
            if (handlers == null)
                return;

            foreach (var handler in handlers.GetInvocationList().OfType<AsyncEventHandler<TEventArgs>>())
                await handler(sender, eventArgs);
        }

        /// <summary>
        /// Raises the event async all at once
        /// </summary>
        /// <typeparam name="TEventArgs">The type of event args</typeparam>
        /// <param name="handlers">The handlers to raise</param>
        /// <param name="sender">The sender</param>
        /// <param name="eventArgs">The event args</param>
        /// <returns>The task</returns>
        public static async Task RaiseAllAsync<TEventArgs>(this AsyncEventHandler<TEventArgs> handlers, object sender, TEventArgs eventArgs)
            where TEventArgs : EventArgs
        {
            if (handlers == null)
                return;

            // Create the task
            var task = Task.WhenAll(handlers.
                // Get the list of event handlers
                GetInvocationList().
                // Get only the specified type we want
                OfType<AsyncEventHandler<TEventArgs>>().
                // Convert each to a task with no parameters
                Select(x => Task.Run((async () => await x.Invoke(sender, eventArgs)))));

            try
            {
                // Await all tasks to complete
                await task;
            }
            catch (Exception)
            {
                // Log every exception if available
                if (task.Exception != null)
                {
                    foreach (var innerEx in task.Exception.InnerExceptions)
                        Logger.Fatal(innerEx, "Async event exception");
                }

                // Throw caught exception
                throw;
            }
        }
    }
}