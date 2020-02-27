using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman.Rayman1;
using RayCarrot.UI;
using RayCarrot.WPF;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RayCarrot.RCP.Metro.Test
{
    /// <summary>
    /// Interaction logic for R1Editor.xaml
    /// </summary>
    public partial class R1Editor : UserControl, IWindowBaseControl<R1EditorViewModel>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="viewModel">The view model</param>
        public R1Editor(R1EditorViewModel viewModel)
        {
            InitializeComponent();

            DataContext = ViewModel = viewModel;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The view model
        /// </summary>
        public R1EditorViewModel ViewModel { get; }

        /// <summary>
        /// The dialog content
        /// </summary>
        public object UIContent => this;

        /// <summary>
        /// Indicates if the dialog should be resizable
        /// </summary>
        public bool Resizable => true;

        /// <summary>
        /// The base size for the dialog
        /// </summary>
        public DialogBaseSize BaseSize => DialogBaseSize.Largest;

        #endregion

        #region Public Methods

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        { }

        #endregion

        #region Events

        /// <summary>
        /// Invoke to request the dialog to close
        /// </summary>
        public event EventHandler CloseDialog;

        #endregion
    }

    public class R1EditorViewModel : UserInputViewModel
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public R1EditorViewModel()
        {
            // Create the commands
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            DuplicateEventCommand = new RelayCommand(DuplicateEvent);

            // Set properties
            Title = "Rayman 1 Editor";
            BaseMapPath = Games.Rayman1.GetInstallDir() + "PCMAP";
            RelativeMapPaths = Directory.GetFiles(BaseMapPath, "*.lev", SearchOption.AllDirectories).Select(x => x - BaseMapPath).ToArray();
            SelectedMapPath = RelativeMapPaths.First();
        }

        private EventViewModel _selectedEvent;

        private FileSystemPath _selectedMapPath;

        /// <summary>
        /// The base map path
        /// </summary>
        public FileSystemPath BaseMapPath { get; }

        /// <summary>
        /// The available relative map paths
        /// </summary>
        public FileSystemPath[] RelativeMapPaths { get; }

        /// <summary>
        /// The currently selected map path
        /// </summary>
        public FileSystemPath SelectedMapPath
        {
            get => _selectedMapPath;
            set
            {
                _selectedMapPath = value;
                UpdateMap();
            }
        }

        /// <summary>
        /// The currently loaded level data
        /// </summary>
        protected Rayman1PCLevData LevData { get; set; }

        /// <summary>
        /// The events for the loaded level data
        /// </summary>
        public ObservableCollection<EventViewModel> Events { get; set; }

        /// <summary>
        /// The level bitmap
        /// </summary>
        public WriteableBitmap LevelBmp { get; set; }

        /// <summary>
        /// The currently selected event
        /// </summary>
        public EventViewModel SelectedEvent
        {
            get => _selectedEvent;
            set
            {
                if (SelectedEvent != null)
                    DrawPoint(SelectedEvent.Event.XPosition, SelectedEvent.Event.YPosition, true);

                _selectedEvent = value;

                if (SelectedEvent != null)
                    DrawPoint(SelectedEvent.Event.XPosition, SelectedEvent.Event.YPosition, false);
            }
        }

        public ICommand SaveCommand { get; }

        public ICommand DuplicateEventCommand { get; }

        /// <summary>
        /// Updates the map
        /// </summary>
        public void UpdateMap()
        {
            LevData = Rayman1PCLevData.GetSerializer().Deserialize(BaseMapPath + SelectedMapPath);
            Events = new ObservableCollection<EventViewModel>();
            SelectedEvent = null;

            for (int i = 0; i < LevData.EventCount; i++)
            {
                Events.Add(new EventViewModel()
                {
                    Link = LevData.EventLinkingTable[i],
                    Event = LevData.Events[i],
                    Command = LevData.EventCommands[i]
                });
            }

            LevelBmp = new WriteableBitmap(LevData.MapWidth * Rayman1PCLevTexture.Size, LevData.MapHeight * Rayman1PCLevTexture.Size, 96, 96, PixelFormats.Bgra32, null);

            // Enumerate each cell
            for (int cellY = 0; cellY < LevData.MapHeight; cellY++)
            {
                for (int cellX = 0; cellX < LevData.MapWidth; cellX++)
                {
                    // Get the cell
                    var cell = LevData.MapCells[cellX, cellY];

                    // Ignore if fully transparent
                    if (cell.TransparencyMode == Rayman1PCLevMapCellTransparencyMode.FullyTransparent)
                        continue;

                    // Get the offset for the texture
                    var texOffset = LevData.TexturesOffsetTable[cell.TextureIndex];

                    // Get the texture
                    var texture = cell.TransparencyMode == Rayman1PCLevMapCellTransparencyMode.NoTransparency ? LevData.NonTransparentTextures.FindItem(x => x.Offset == texOffset) : LevData.TransparentTextures.FindItem(x => x.Offset == texOffset);

                    byte[] buffer = new byte[Rayman1PCLevTexture.Size * Rayman1PCLevTexture.Size * 4];

                    // Write each pixel for the texture
                    for (int x = 0; x < Rayman1PCLevTexture.Size; x++)
                    {
                        for (int y = 0; y < Rayman1PCLevTexture.Size; y++)
                        {
                            // NOTE: The color palette isn't always correct - the palette changer event needs to be checked
                            // Get the color
                            var c = LevData.ColorPalettes[0][texture.ColorIndexes[x, y]];

                            byte blue = c.B;
                            byte green = c.G;
                            byte red = c.R;
                            byte alpha = Byte.MaxValue;

                            // If the texture is transparent, replace the color with one with the alpha channel
                            if (texture is Rayman1PCLevTransparentTexture tt)
                                alpha = tt.Alpha[x, y];

                            // NOTE: Transparent textures aren't handled
                            // Set the pixel
                            buffer[(y * Rayman1PCLevTexture.Size + x) * 4 + 0] = blue;
                            buffer[(y * Rayman1PCLevTexture.Size + x) * 4 + 1] = green;
                            buffer[(y * Rayman1PCLevTexture.Size + x) * 4 + 2] = red;
                            buffer[(y * Rayman1PCLevTexture.Size + x) * 4 + 3] = alpha;
                        }
                    }

                    var bmpX = Rayman1PCLevTexture.Size * cellX;
                    var bmpY = Rayman1PCLevTexture.Size * cellY;

                    LevelBmp.WritePixels(new Int32Rect(bmpX, bmpY, Rayman1PCLevTexture.Size, Rayman1PCLevTexture.Size), buffer, Rayman1PCLevTexture.Size * 4, 0);
                }
            }

            foreach (var e in LevData.Events)
                DrawPoint(e.XPosition, e.YPosition, true);
        }

        /// <summary>
        /// Draws a point on the bitmap
        /// </summary>
        /// <param name="bmpX">The x position</param>
        /// <param name="bmpY">The y position</param>
        /// <param name="isRed">True if it's red, false if it's blue</param>
        public void DrawPoint(uint bmpX, uint bmpY, bool isRed)
        {
            byte[] buffer = new byte[4 * 4 * 4];

            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    byte blue = 0;
                    byte green = 0;
                    byte red = Byte.MaxValue;
                    byte alpha = Byte.MaxValue;

                    if (!isRed)
                    {
                        blue = Byte.MaxValue;
                        red = 0;
                    }

                    // Set the pixel
                    buffer[(y * 4 + x) * 4 + 0] = blue;
                    buffer[(y * 4 + x) * 4 + 1] = green;
                    buffer[(y * 4 + x) * 4 + 2] = red;
                    buffer[(y * 4 + x) * 4 + 3] = alpha;
                }
            }

            try
            {
                LevelBmp.WritePixels(new Int32Rect((int)bmpX, (int)bmpY, 4, 4), buffer, 4 * 4, 0);

                RCFCore.Logger?.LogInformationSource("Success");
            }
            catch (Exception ex)
            {
                ex.HandleError();
            }
        }


        public async Task SaveAsync()
        {
            var levFilePath = BaseMapPath + SelectedMapPath;
            var backupPath = levFilePath.AppendFileExtension(new FileExtension(".BACKUP"));

            if (!backupPath.FileExists)
                RCFRCP.File.CopyFile(levFilePath, backupPath, false);

            LevData.Events.Clear();
            LevData.Events.AddRange(Events.Select(x => x.Event));
            LevData.EventLinkingTable.Clear();
            LevData.EventLinkingTable.AddRange(Events.Select(x => x.Link));
            LevData.EventCommands.Clear();
            LevData.EventCommands.AddRange(Events.Select(x => x.Command));
            LevData.EventCount = (ushort)LevData.Events.Count;
            
            Rayman1PCLevData.GetSerializer().Serialize(levFilePath, LevData);

            await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync("Saved");
        }

        public void DuplicateEvent()
        {
            if (SelectedEvent == null)
                return;

            Events.Add(new EventViewModel()
            {
                Command = SelectedEvent.Command,
                Link = SelectedEvent.Link,
                Event = SelectedEvent.Event.Clone().CastTo<Rayman1PCLevEvent>()
            });

            foreach (var e in Events)
            {
                e.Link = (ushort)Events.IndexOf(e);
            }
        }


        public class EventViewModel : BaseViewModel
        {
            public ushort Link { get; set; }

            public Rayman1PCLevEvent Event { get; set; }

            public Rayman1PCLevEventCommand Command { get; set; }
            
        }
    }
}