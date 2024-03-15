using System.Diagnostics;
using System.IO;
using System.Windows.Input;
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

        // TODO-UPDATE: Do we still need this?
        // Hack to fix a weird binding issue where the first int box gets set to 0
        //MemContainer.AccessMemory(m =>
        //{
        //    m.ClearModifiedValues();
        //    RefreshFields();
        //    m.ClearModifiedValues();
        //});

        // Create commands
        RefreshLogCommand = new RelayCommand(RefreshLog);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand RefreshLogCommand { get; }

    #endregion

    #region Private Properties

    public MemoryDataContainer MemContainer { get; }
    public bool LogNextTick { get; private set; }

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
        MemorySerializerLogger? logger = Context.SerializerLogger as MemorySerializerLogger;

        bool log = LogNextTick; // Only read once since it might get changed during this function!
        if (log && logger != null)
            logger.IsEnabled = true;

        MemContainer.Update();

        if (!MemContainer.Validate())
            throw new Exception("Serialized memory data was invalid");

        if (log && logger != null)
        {
            logger.IsEnabled = false;
            LogNextTick = false;
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

    public void RefreshLog()
    {
        LogNextTick = true;
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