using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using BinarySerializer;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro.Games.Tools.RuntimeModifications;

// TODO-UPDATE: Update UI. Make more user-friendly - auto search game by default. Improve finding process.
public class RuntimeModificationsViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    public RuntimeModificationsViewModel(GameInstallation gameInstallation, IMessageUIManager messageUi)
    {
        GameInstallation = gameInstallation;
        MessageUI = messageUi ?? throw new ArgumentNullException(nameof(messageUi));

        ProcessAttacherViewModel = new ProcessAttacherViewModel();
        ProcessAttacherViewModel.Refreshed += (_, _) => AutoSelectGame();
        ProcessAttacherViewModel.ProcessAttached += (_, e) => AttachProcess(e.AttachedProcess);
        ProcessAttacherViewModel.ProcessDetached += (_, _) => DetachProcess();

        // Get the available game versions from components. Some games have different versions, such as Rayman 1 on PC,
        // or different regional releases, such as most console games, where memory offsets will differ.
        GameVersions = new ObservableCollection<GameVersionViewModel>(GameInstallation.
            GetComponents<RuntimeModificationsManagersComponent>().
            CreateManyObjects().
            Select(x => new GameVersionViewModel(x)));
        SelectedGameVersion = GameVersions.First();

        // Get the available emulator versions. We could have these be registered as components, but then we won't get
        // any emulator versions if the game is launched through a non-emulator client such as Ubisoft Connect.
        EmulatorVersion[] emulatorVersions = GameInstallation.GetRequiredComponent<RuntimeModificationsManagersComponent>().EmulatedPlatform switch
        {
            EmulatedPlatform.None => EmulatorVersion.None,
            EmulatedPlatform.MsDos => EmulatorVersion.MsDos,
            EmulatedPlatform.Gba => EmulatorVersion.Gba,
            EmulatedPlatform.Ps1 => EmulatorVersion.Ps1,
            _ => throw new ArgumentOutOfRangeException()
        };
        EmulatorVersions = new ObservableCollection<EmulatorVersionViewModel>(emulatorVersions.Select(x => new EmulatorVersionViewModel(x)));
        SelectedEmulatorVersion = EmulatorVersions.First();
        
        ProcessAttacherViewModel.ProcessNameKeywords = GameVersions.
            SelectMany(x => x.Manager.ProcessNameKeywords).
            Concat(EmulatorVersions.SelectMany(x => x.EmulatorVersion.ProcessNameKeywords)).
            Distinct().
            ToArray();

        EditorFieldGroups = new ObservableCollection<EditorFieldGroupViewModel>();
        InfoItems = new ObservableCollection<DuoGridItemViewModel>();
        Actions = new ObservableCollection<ActionViewModel>();

        EditorFieldGroups.EnableCollectionSynchronization();
        InfoItems.EnableCollectionSynchronization();
        Actions.EnableCollectionSynchronization();

        RefreshLogCommand = new RelayCommand(RefreshLog);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private CancellationTokenSource? _updateCancellation;
    private MemoryDataContainer? _memContainer;
    private bool _logNextTick;

    #endregion

    #region Services

    public IMessageUIManager MessageUI { get; }

    #endregion

    #region Commands

    public ICommand RefreshLogCommand { get; }

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }
    public ProcessAttacherViewModel ProcessAttacherViewModel { get; }
    public Context? Context { get; private set; }

    public ObservableCollection<GameVersionViewModel> GameVersions { get; }
    public GameVersionViewModel SelectedGameVersion { get; set; }

    public ObservableCollection<EmulatorVersionViewModel>? EmulatorVersions { get; set; }
    public EmulatorVersionViewModel SelectedEmulatorVersion { get; set; }

    public RuntimeModificationsManager? AttachedGame { get; set; }

    public ObservableCollection<EditorFieldGroupViewModel> EditorFieldGroups { get; }
    public ObservableCollection<DuoGridItemViewModel> InfoItems { get; }
    public ObservableCollection<ActionViewModel> Actions { get; }
    public bool HasActions { get; set; }
    public string? Log { get; set; }

    #endregion

    #region Private Methods

    private void DisposeContext()
    {
        if (Context == null)
            return;

        // Dispose the streams
        foreach (ProcessMemoryStreamFile file in Context.MemoryMap.Files.OfType<ProcessMemoryStreamFile>())
            file.DisposeStream();

        Context.Dispose();
    }

    private void AutoSelectGame()
    {
        if (ProcessAttacherViewModel.SelectedProcess == null)
            return;

        string processName = ProcessAttacherViewModel.SelectedProcess.ProcessName;

        // TODO-UPDATE: Re-implement
        //foreach (GameVersionViewModel game in Games)
        //{
        //    if (game.ProcessNameKeywords.Concat(game.Emulators.SelectMany(x => x.ProcessNameKeywords)).
        //        Any(x => processName.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) != -1))
        //    {
        //        SelectedGameVersion = game;
        //        return;
        //    }
        //}
    }

    private async void AttachProcess(AttachableProcessViewModel p)
    {
        try
        {
            // Create a new context
            DisposeContext();
            
            AttachedGame?.DetachContainer();
            AttachedGame = SelectedGameVersion.Manager;
            
            Context = new RCPContext(String.Empty, logger: new MemorySerializerLogger()
            {
                IsEnabled = false, // Start disabled as we only want to log individual ticks
            });

            MemoryData memData = AttachedGame.CreateMemoryData(Context);
            _memContainer = new MemoryDataContainer(memData);

            AttachedGame.AttachContainer(_memContainer);
            AttachedGame.InitializeContext(Context);

            BinaryDeserializer s = Context.Deserializer;

            foreach (MemoryRegion memRegion in SelectedEmulatorVersion.EmulatorVersion.MemoryRegions)
            {
                // Open the process as a stream
                ProcessMemoryStream stream = new(p.Process, ProcessMemoryStream.Mode.AllAccess);

                var file = Context.AddFile(new ProcessMemoryStreamFile(
                    context: Context, 
                    name: memRegion.Name, 
                    baseAddress: memRegion.GameOffset,
                    memoryRegionLength: memRegion.Length,
                    stream: new BufferedStream(stream), 
                    mode: VirtualFileMode.DoNotClose));

                // Initialize the memory stream
                s.Goto(file.StartPointer);
                InitializeProcessStream(stream, memRegion, s);
            }

            memData.Initialize(Context, SelectedGameVersion.Manager.GetOffsets());

            // Initialize the fields
            EditorFieldGroups.Clear();
            InfoItems.Clear();
            Actions.Clear();
            Log = null;

            EditorFieldGroups.AddRange(AttachedGame.CreateEditorFieldGroups());
            InfoItems.AddRange(AttachedGame.CreateInfoItems());
            Actions.AddRange(AttachedGame.CreateActions());
            HasActions = Actions.Any();

            // Hack to fix a weird binding issue where the first int box gets set to 0
            _memContainer.AccessMemory(m =>
            {
                m.ClearModifiedValues();
                RefreshFields();
                m.ClearModifiedValues();
            });
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Attaching to process for memory loading");

            await MessageUI.DisplayMessageAsync(Resources.Mod_Mem_AttachError, MessageType.Error);

            await ProcessAttacherViewModel.DetachProcessAsync();
            return;
        }

        // Create a cancellation source
        _updateCancellation = new CancellationTokenSource();
        CancellationToken token = _updateCancellation.Token;

        // IDEA: Use DispatcherTimer instead? That will cause the tick to occur on the UI thread which solves some threading issues
        //       and removes the need to use locks. However we get slightly less control over how and when the ticks occur and if
        //       the code is too slow it will impact the UI.

        // Start refreshing
        _ = Task.Run(async () =>
        {
            try
            {
                while (true)
                {
                    RefreshFields();

                    // The games only update 60 frames per second, so we do the same
                    await Task.Delay(TimeSpan.FromSeconds(1 / 60d), token);
                }
            }
            catch (OperationCanceledException ex)
            {
                Logger.Debug(ex, "Updating memory mod fields");
            }
            catch (Exception ex)
            {
                // Wait a bit in case the process is currently exiting so we don't have to show an error message
                await Task.Delay(TimeSpan.FromSeconds(1));

                if (ProcessAttacherViewModel.AttachedProcess?.Process.HasExited == true)
                {
                    Logger.Debug(ex, "Updating memory mod fields");
                }
                else
                {
                    Logger.Warn(ex, "Updating memory mod fields");

                    await MessageUI.DisplayMessageAsync(Resources.Mod_Mem_TickError, MessageType.Error);
                }

                await ProcessAttacherViewModel.DetachProcessAsync();
            }
        });
    }

    private void DetachProcess()
    {
        _updateCancellation?.Cancel();
        _memContainer = null;
        AttachedGame?.DetachContainer();

        EditorFieldGroups.Clear();
        InfoItems.Clear();
        Actions.Clear();
        Log = null;
    }

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
    }

    private void RefreshFields()
    {
        MemorySerializerLogger? logger = Context?.SerializerLogger as MemorySerializerLogger;

        if (_logNextTick && logger != null)
            logger.IsEnabled = true;

        _memContainer?.Update();

        if (_logNextTick && logger != null)
        {
            logger.IsEnabled = false;
            _logNextTick = false;
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

    #endregion

    #region Public Methods

    // TODO-UPDATE: Re-implement
    //public override async Task InitializeAsync()
    //{
    //    await ProcessAttacherViewModel.RefreshProcessesAsync();
    //}

    public void RefreshLog()
    {
        _logNextTick = true;
    }

    public virtual void Dispose()
    {
        _updateCancellation?.Cancel();
        _updateCancellation?.Dispose();
        ProcessAttacherViewModel.Dispose();
        _memContainer = null;
        AttachedGame?.DetachContainer();
        AttachedGame = null;
        DisposeContext();
    }

    #endregion
}