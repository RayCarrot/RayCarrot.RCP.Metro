using System;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// An event handler for async events which return a <see cref="Task"/>
/// </summary>
/// <typeparam name="TEventArgs">The type of event args</typeparam>
/// <param name="sender">The sender</param>
/// <param name="eventArgs">The event args</param>
/// <returns>The task</returns>
public delegate Task AsyncEventHandler<in TEventArgs>(object sender, TEventArgs eventArgs)
    where TEventArgs : EventArgs;