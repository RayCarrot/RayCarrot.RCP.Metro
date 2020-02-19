using System;
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro
{
    // TODO: Move to framework?
    /// <summary>
    /// A list which can be disposed
    /// </summary>
    /// <typeparam name="T">The type of disposable items in the list</typeparam>
    public class DisposableList<T> : List<T>, IDisposable
        where T : IDisposable
    {
        /// <summary>
        /// Disposes every item in the list
        /// </summary>
        public void Dispose()
        {
            foreach (T item in this)
                item?.Dispose();
        }
    }
}