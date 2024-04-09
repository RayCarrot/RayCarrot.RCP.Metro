namespace RayCarrot.RCP.Metro;

public static class BinaryHelpers
{
    private static readonly string[] Units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

    // We use binary units, but display them without the i in the name to match what Windows does
    public static string BytesToString(long size)
    {
        double value = size;
        int unitIndex = 0;
        while (value >= 1024)
        {
            value /= 1024;
            ++unitIndex;
        }

        string unit = Units[unitIndex];
        return $"{value:0.##} {unit}";
    }
}