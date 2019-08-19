using System;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Extensions methods for <see cref="AsyncEventHandler{TEventArgs}"/>
    /// </summary>
    public static class AsyncEventHandlertExtensions
    {
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

            foreach (AsyncEventHandler<TEventArgs> handler in handlers.GetInvocationList())
                await handler(sender, eventArgs);
        }
    }
}