namespace RayCarrot.RCP.Metro;

// Might be more flags
[Flags]
public enum RHR_LevelSaveFlags : byte
{
    None = 0,
    Unlocked = 1 << 0,
    Completed = 1 << 1,
}