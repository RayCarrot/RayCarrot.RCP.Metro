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

                Display.DEVMODE vDevMode = new();
                var distinctGraphicsModes = new List<GraphicsMode>();

                int i = 0;
                while (Display.EnumDisplaySettings(null, i, ref vDevMode))
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
                    {
                        if (!IncludeRefreshRate)
                            refreshRate = 0;

                        if (!distinctGraphicsModes.Any(x => x.Width == width && x.Height == height && x.RefreshRate == refreshRate))
                            distinctGraphicsModes.Add(new GraphicsMode(width, height, refreshRate));
                    }

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