using System;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Event arguments for when a property is changed
    /// </summary>
    /// <typeparam name="T">The type of property</typeparam>
    public class PropertyChangedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="oldValue">The old value</param>
        /// <param name="newValue">The new value</param>
        public PropertyChangedEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// The old value
        /// </summary>
        public T OldValue { get; }

        /// <summary>
        /// The new value
        /// </summary>
        public T NewValue { get; }
    }

    /// <summary>
    /// Event handler for when a property is changed
    /// </summary>
    /// <typeparam name="T">The type of property</typeparam>
    /// <param name="sender">The sender</param>
    /// <param name="e">The event arguments</param>
    public delegate void PropertyChangedEventHandler<T>(object sender, PropertyChangedEventArgs<T> e);
}