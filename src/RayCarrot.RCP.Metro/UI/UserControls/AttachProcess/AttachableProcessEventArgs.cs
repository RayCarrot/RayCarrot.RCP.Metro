using System;

namespace RayCarrot.RCP.Metro;

public class AttachableProcessEventArgs : EventArgs
{
    public AttachableProcessEventArgs(AttachableProcessViewModel attachedProcess)
    {
        AttachedProcess = attachedProcess;
    }

    public AttachableProcessViewModel AttachedProcess { get; }
}