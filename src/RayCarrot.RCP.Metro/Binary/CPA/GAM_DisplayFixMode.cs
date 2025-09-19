namespace RayCarrot.RCP.Metro;

[Flags]
public enum GAM_DisplayFixMode : byte
{
    Nothing = 0,
    HitPoints = 1 << 0,
    GameSave = 1 << 1,
    All = 0xFF,
}