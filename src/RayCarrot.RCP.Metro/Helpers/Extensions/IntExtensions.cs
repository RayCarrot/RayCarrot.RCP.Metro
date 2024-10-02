namespace RayCarrot.RCP.Metro;

public static class IntExtensions
{
    public static bool IsPowerOfTwo(this int value)
    {
        return value > 0 && (value & (value - 1)) == 0;
    }
}