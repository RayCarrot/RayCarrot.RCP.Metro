#nullable disable
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using PropertyChanged;

namespace RayCarrot.RCP.Metro;

public class GraphicsModeSelectionViewModel : BaseRCPViewModel
{
    #region Constructor

    public GraphicsModeSelectionViewModel()
    {
        GraphicsModes = new ObservableCollection<GraphicsMode>();
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private GraphicsMode _selectedGraphicsMode;

    #endregion

    #region Public Properties

    public ObservableCollection<GraphicsMode> GraphicsModes { get; }
        
    public bool AllowCustomGraphicsMode { get; set; } = true;
    public bool IncludeRefreshRate { get; set; } = false;

    public GraphicsSortMode SortMode { get; set; } = GraphicsSortMode.WidthThenHeight;
    public GraphicsSortDirection SortDirection { get; set; } = GraphicsSortDirection.Ascending;
    public GraphicsFilterMode FilterMode { get; set; } = GraphicsFilterMode.WidthAndHeight;

    public int MinGraphicsWidth { get; set; } = 0;
    public int MinGraphicsHeight { get; set; } = 0;

    public int MaxGraphicsWidth { get; set; } = Int32.MaxValue;
    public int MaxGraphicsHeight { get; set; } = Int32.MaxValue;

    public int Width => SelectedGraphicsMode?.Width ?? 0;
    public int Height => SelectedGraphicsMode?.Height ?? 0;

    public GraphicsMode SelectedGraphicsMode
    {
        get => _selectedGraphicsMode;
        set
        {
            _selectedGraphicsMode = value;
            OnGraphicsModeChanged();
        }
    }

    public int SelectedGraphicsModeIndex
    {
        get => GraphicsModes.IndexOf(SelectedGraphicsMode);
        set => SelectedGraphicsMode = GraphicsModes.ElementAtOrDefault(value);
    }

    #endregion

    #region Public Methods

    public void GetAvailableResolutions()
    {
        lock (GraphicsModes)
        {
            Metro.App.Current.Dispatcher.Invoke(() =>
            {
                Logger.Info("Refreshing resolution selection graphics modes");

                GraphicsModes.Clear();

                DEVMODE vDevMode = new DEVMODE();
                var distinctGraphicsModes = new HashSet<GraphicsMode>();

                int i = 0;
                while (EnumDisplaySettings(null, i, ref vDevMode))
                {
                    int width = vDevMode.dmPelsWidth;
                    int height = vDevMode.dmPelsHeight;
                    int refreshRate = vDevMode.dmDisplayFrequency;

                    bool isValid = FilterMode switch
                    {
                        GraphicsFilterMode.WidthAndHeight => width >= MinGraphicsWidth &&
                                                             height >= MinGraphicsHeight &&
                                                             width <= MaxGraphicsWidth &&
                                                             height <= MaxGraphicsHeight,
                        GraphicsFilterMode.WidthOrHeight => (width >= MinGraphicsWidth ||
                                                             height >= MinGraphicsHeight) &&
                                                            (width <= MaxGraphicsWidth ||
                                                             height <= MaxGraphicsHeight),
                        _ => throw new Exception($"Filter mode {FilterMode} is not valid")
                    };

                    if (isValid)
                        distinctGraphicsModes.Add(new GraphicsMode(width, height, IncludeRefreshRate ? refreshRate : 0));

                    i++;
                }

                IEnumerable<GraphicsMode> graphicsModes = distinctGraphicsModes;

                // Sort
                graphicsModes = SortMode switch
                {
                    GraphicsSortMode.WidthThenHeight => graphicsModes.OrderBy(x => x.Width).ThenBy(x => x.Height).ThenBy(x => x.RefreshRate),
                    GraphicsSortMode.TotalPixels => graphicsModes.OrderBy(x => x.Width * x.Height).ThenBy(x => x.RefreshRate),
                    _ => throw new Exception($"Graphics sort mode {SortMode} is not valid")
                };

                // Reverse if descending
                if (SortDirection == GraphicsSortDirection.Descending)
                    graphicsModes = graphicsModes.Reverse();

                GraphicsModes.AddRange(graphicsModes);

                Logger.Info("Refreshed resolution selection graphics modes");
            });
        }
    }

    #endregion

    #region P/Invoke

    [DllImport("user32.dll")]
    private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

    [StructLayout(LayoutKind.Sequential)]
    public struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public int dmPositionX;
        public int dmPositionY;
        public int dmDisplayOrientation;
        public int dmDisplayFixedOutput;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
        public string dmFormName;
        public short dmLogPixels;
        public int dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;
    }

    #endregion

    #region Events

    [SuppressPropertyChangedWarnings]
    protected virtual void OnGraphicsModeChanged() => GraphicsModeChanged?.Invoke(this, EventArgs.Empty);

    public event EventHandler GraphicsModeChanged;

    #endregion

    #region Enums

    public enum GraphicsSortMode
    {
        WidthThenHeight,
        TotalPixels,
    }

    public enum GraphicsSortDirection
    {
        Ascending,
        Descending,
    }

    public enum GraphicsFilterMode
    {
        WidthAndHeight,
        WidthOrHeight,
    }

    #endregion
}