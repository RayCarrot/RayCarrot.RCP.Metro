#nullable disable
namespace RayCarrot.RCP.Metro;

public record GraphicsMode
{
    public GraphicsMode(int width, int height, int refreshRate = 0)
    {
        Width = width;
        Height = height;
        RefreshRate = refreshRate;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public int Width { get; }
    public int Height { get; }
    public int RefreshRate { get; }

    public static bool TryParse(string value, out GraphicsMode g)
    {
        // Default to null
        g = null;

        if (value == null)
        {
            Logger.Debug("Failed parsing resolution value due to it being null");
            return false;
        }

        string[] values = value.Split('x').Select(x => x.Trim()).ToArray();

        if (values.Length != 2)
        {
            Logger.Debug("Failed parsing resolution value {0} due to it not containing a single x", value);
            return false;
        }

        int width;
        int height;
        int refreshRate = 0;

        // Get the width
        if (Int32.TryParse(values[0], out int w))
        {
            width = w;
        }
        else
        {
            Logger.Debug("Failed parsing resolution value {0} due to the width not being valid", value);
            return false;
        }

        // Check if the refresh rate is included
        var refreshRateStart = values[1].IndexOf('(');
        var refreshRateEnd = values[1].IndexOf(')');

        if (refreshRateStart != -1 && refreshRateEnd != -1 && refreshRateStart < refreshRateEnd)
        {
            // Get the refresh rate
            var refreshRateStr = values[1].Substring(refreshRateStart + 1, refreshRateEnd - refreshRateStart - 1).Trim();

            if (Int32.TryParse(refreshRateStr, out int r))
            {
                refreshRate = r;
            }
            else
            {
                Logger.Debug("Failed parsing resolution value {0} due to the refresh rate not being valid", value);
                return false;
            }

            // Correct the height value
            values[1] = values[1].Substring(0, refreshRateStart).Trim();
        }

        // Get the height
        if (Int32.TryParse(values[1], out int h))
        {
            height = h;
        }
        else
        {
            Logger.Debug("Failed parsing resolution value {0} due to the height not being valid", value);
            return false;
        }

        Logger.Debug("Correctly parsed resolution value {0} as W: {1} H: {2} R: {3}", value, width, height, refreshRate);

        g = new GraphicsMode(width, height, refreshRate);

        return true;
    }

    public override string ToString()
    {
        var str = $"{Width} x {Height}";

        if (RefreshRate > 0)
            str += $" ({RefreshRate})";

        return str;
    }
}