﻿using System;
using System.ComponentModel;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// An abstract window instance
    /// </summary>
    public abstract class WindowInstance
    {
        public abstract string Title { get; set; }
        public abstract bool CanClose { get; set; }

        public abstract void Close();
        public abstract void Focus();

        public abstract event EventHandler<CancelEventArgs> WindowClosing;
        public abstract event EventHandler WindowClosed;
    }
}