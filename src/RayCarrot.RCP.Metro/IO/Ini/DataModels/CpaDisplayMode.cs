namespace RayCarrot.RCP.Metro.Ini;

public readonly struct CpaDisplayMode
{
    public CpaDisplayMode(bool isFullscreen, int width, int height, int bitsPerPixel)
    {
        IsFullscreen = isFullscreen;
        Width = width;
        Height = height;
        BitsPerPixel = bitsPerPixel;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public bool IsFullscreen { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int BitsPerPixel { get; init; }

    public static bool TryParse(string value, out CpaDisplayMode displayMode)
    {
        try
        {
            // Template:
            // 1 - 1920 x 1080 x 16

            // Split the values
            string[] values = value.Split('x', '-');

            // Trim the values
            for (int i = 0; i < values.Length; i++)
                values[i] = values[i].Trim();

            displayMode = new CpaDisplayMode()
            {
                IsFullscreen = Int32.Parse(values[0]) != 0,
                Width = Int32.Parse(values[1]),
                Height = Int32.Parse(values[2]),
                BitsPerPixel = Int32.Parse(values[3]),
            };

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Parsing CPA display mode string");
            displayMode = default;
            return false;
        }
    }

    public override string ToString()
    {
        return $"{(IsFullscreen ? 1 : 0)} - {Width} x {Height} x {BitsPerPixel}";
    }
}