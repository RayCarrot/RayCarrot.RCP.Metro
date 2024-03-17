using System.Diagnostics;
using System.IO;
using BinarySerializer;

namespace RayCarrot.RCP.Metro.Games.Tools.RuntimeModifications;

public class RunningGameViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    public RunningGameViewModel(GameManager gameManager, EmulatorManager emulatorManager, Process process)
    {
        GameManager = gameManager;
        ProcessViewModel = new ProcessViewModel(process, ShellThumbnailSize.Medium);

        Context = new RCPContext(String.Empty, logger: new MemorySerializerLogger()
        {
            IsEnabled = false, // Start disabled as we only want to log individual ticks
        });

        MemoryData memData = GameManager.CreateMemoryData(Context);
        MemContainer = new MemoryDataContainer(memData);

        GameManager.AttachContainer(MemContainer);
        GameManager.InitializeContext(Context);

        BinaryDeserializer s = Context.Deserializer;

        foreach (MemoryRegion memRegion in emulatorManager.MemoryRegions)
        {
            // Open the process as a stream
            ProcessMemoryStream stream = new(process, ProcessMemoryStream.Mode.AllAccess);

            ProcessMemoryStreamFile file = new(
                context: Context,
                name: memRegion.Name,
                baseAddress: memRegion.GameOffset,
                memoryRegionLength: memRegion.Length,
                stream: new BufferedStream(stream),
                mode: VirtualFileMode.DoNotClose);

            Context.AddFile(file);

            // Initialize the memory stream
            s.Goto(file.StartPointer);
            InitializeProcessStream(stream, memRegion, s);
        }

        memData.Initialize(Context, GameManager.GetOffsets());

        // Create the fields
        EditorFieldGroups = new ObservableCollection<EditorFieldGroupViewModel>(GameManager.CreateEditorFieldGroups());
        InfoItems = new ObservableCollection<DuoGridItemViewModel>(GameManager.CreateInfoItems());
        Actions = new ObservableCollection<ActionViewModel>(GameManager.CreateActions());
        HasActions = Actions.Any();

        EditorFieldGroups.EnableCollectionSynchronization();
        InfoItems.EnableCollectionSynchronization();
        Actions.EnableCollectionSynchronization();
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Properties

    public MemoryDataContainer MemContainer { get; }

    #endregion

    #region Public Properties

    public Context Context { get; }
    public GameManager GameManager { get; }
    public ProcessViewModel ProcessViewModel { get; }
    public ObservableCollection<EditorFieldGroupViewModel> EditorFieldGroups { get; }
    public ObservableCollection<DuoGridItemViewModel> InfoItems { get; }
    public ObservableCollection<ActionViewModel> Actions { get; }
    public bool HasActions { get; set; }
    public string? Log { get; set; }
    public bool LogEnabled
    {
        get => Context.SerializerLogger is MemorySerializerLogger { IsEnabled: true };
        set
        {
            if (Context.SerializerLogger is MemorySerializerLogger logger)
                logger.IsEnabled = value;
        }
    }

    #endregion

    #region Private Methods

    private static void InitializeProcessStream(ProcessMemoryStream stream, MemoryRegion memRegion, BinaryDeserializer s)
    {
        long moduleOffset = 0;

        // We could also set the offset for the main module, but we assume it's always 0x400000
        if (memRegion.ModuleName != null)
            moduleOffset = stream.Process.Modules.Cast<ProcessModule>().First(x => x.ModuleName == memRegion.ModuleName).BaseAddress.ToInt64();

        long baseStreamOffset;

        if (memRegion.IsProcessOffsetAPointer)
        {
            Pointer basePtrPtr = s.CurrentPointer + memRegion.ProcessOffset + moduleOffset;

            // Get the base pointer
            baseStreamOffset = stream.Is64Bit
                ? s.DoAt(basePtrPtr, () => s.Serialize<long>(default))
                : s.DoAt(basePtrPtr, () => s.Serialize<uint>(default));
        }
        else
        {
            baseStreamOffset = memRegion.ProcessOffset + moduleOffset;
        }

        stream.BaseStreamOffset = baseStreamOffset;

        Logger.Info("Initialized process stream for region {0} at 0x{1:X8}", memRegion.Name, baseStreamOffset);
    }

    #endregion

    #region Public Methods

    public void RefreshFields()
    {
        MemContainer.Update();

        if (!MemContainer.Validate())
            throw new Exception("Serialized memory data was invalid");

        if (Context.SerializerLogger is MemorySerializerLogger { IsEnabled: true } logger)
        {
            Log = logger.GetString();
            logger.Clear();
        }

        foreach (EditorFieldGroupViewModel group in EditorFieldGroups)
            group.Refresh();

        foreach (DuoGridItemViewModel item in InfoItems)
            if (item.Text is GeneratedLocString g)
                g.RefreshValue();

        foreach (ActionViewModel action in Actions)
            action.Refresh();
    }

    public void Dispose()
    {
        // Dispose the streams
        foreach (ProcessMemoryStreamFile file in Context.MemoryMap.Files.OfType<ProcessMemoryStreamFile>())
            file.DisposeStream();

        Context.Dispose();
        GameManager.DetachContainer();
        ProcessViewModel.Process.Dispose();
    }

    #endregion
}