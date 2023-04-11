namespace RayCarrot.RCP.Metro;

// Might be more flags but I only found these
[Flags]
public enum RHR_SaveSlotFlags : byte
{
    None = 0,
    InUse = 1 << 0,
    SeenIntro = 1 << 1,
}