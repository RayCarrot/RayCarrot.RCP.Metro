namespace RayCarrot.RCP.Metro;

// Might be more flags but I only found these being checked against
[Flags]
public enum RHR_SaveFlags : byte
{
    None = 0,
    UnlockedCredits = 1 << 0,
    DebugMode = 1 << 1,
}