namespace RayCarrot.RCP.Metro;

// Might be more flags but I only found these being checked against. Also
// not 100% sure about the conditions, but seems reasonable.
[Flags]
public enum RHR_SaveFlags : byte
{
    None = 0,
    GameCompleted = 1 << 0, // Unlocks credits
    GameFullyCompleted = 1 << 1, // Unlocks green pause menu buttons
}