using System;

namespace RayCarrot.RCP.Metro.Ini
{
    /// <summary>
    /// Formats a Rayman GLI_Mode key
    /// </summary>
    public class RayGLI_Mode
    {
        /// <summary>
        /// True if windowed, false if not
        /// </summary>
        public bool IsWindowed
        {
            get => Windowed == 0;
            set => Windowed = value ? 0 : 1;
        }

        /// <summary>
        /// 0 if windowed, 1 if not
        /// </summary>
        public int Windowed { get; set; }

        /// <summary>
        /// The horizontal resolution
        /// </summary>
        public int ResX { get; set; }

        /// <summary>
        /// The vertical resolution
        /// </summary>
        public int ResY { get; set; }

        /// <summary>
        /// The color mode, either 16 bit or 32 bit
        /// </summary>
        public int ColorMode { get; set; }

        public override string ToString()
        {
            return $"{Windowed} - {ResX} x {ResY} x {ColorMode}";
        }

        /// <summary>
        /// Creates a <see cref="RayGLI_Mode"/> from a string
        /// </summary>
        /// <param name="value">The value from a INI key</param>
        /// <returns>The <see cref="RayGLI_Mode"/> or null if it could not be parsed</returns>
        public static RayGLI_Mode? Parse(string? value)
        {
            if (value == null)
                return null;

            try
            {
                // Template:
                // 1 - 1920 x 1080 x 16

                // Split the values
                var values = value.Split('x', '-');

                // Trim the values
                for (int i = 0; i < values.Length; i++)
                    values[i] = values[i].Trim(' ');

                return new RayGLI_Mode()
                {
                    Windowed = Int32.Parse(values[0]),
                    ResX = Int32.Parse(values[1]),
                    ResY = Int32.Parse(values[2]),
                    ColorMode = Int32.Parse(values[3]),
                };

            }
            catch
            {
                // TODO: Log exception
                return null;
            }
        }
    }
}