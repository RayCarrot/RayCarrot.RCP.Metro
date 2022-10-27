using System;

namespace RayCarrot.RCP.Metro;

public abstract class EmulatedPlatformManager : PlatformManager
{
    protected EmulatedPlatformManager(GameDescriptor gameDescriptor) : base(gameDescriptor) { }

    public virtual Emulator[] Emulators => Array.Empty<Emulator>();
}