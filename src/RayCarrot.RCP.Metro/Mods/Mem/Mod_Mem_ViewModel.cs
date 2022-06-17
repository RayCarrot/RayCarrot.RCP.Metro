using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using BinarySerializer;
using BinarySerializer.OpenSpace;
using BinarySerializer.Ray1;
using NLog;

namespace RayCarrot.RCP.Metro;

public class Mod_Mem_ViewModel : Mod_BaseViewModel, IDisposable
{
    #region Constructor

    public Mod_Mem_ViewModel(IMessageUIManager messageUi)
    {
        MessageUI = messageUi ?? throw new ArgumentNullException(nameof(messageUi));

        ProcessAttacherViewModel = new ProcessAttacherViewModel();
        ProcessAttacherViewModel.Refreshed += (_, _) => AutoSelectGame();
        ProcessAttacherViewModel.ProcessAttached += (_, e) => AttachProcess(e.AttachedProcess);
        ProcessAttacherViewModel.ProcessDetached += (_, _) => DetachProcess();

        Mod_Mem_EmulatorViewModel[] emuNone = Mod_Mem_EmulatorViewModel.None;
        Mod_Mem_EmulatorViewModel[] emuMSDOS = Mod_Mem_EmulatorViewModel.MSDOS;
        Mod_Mem_EmulatorViewModel[] emuPS1 = Mod_Mem_EmulatorViewModel.PS1;
        Mod_Mem_EmulatorViewModel[] emuGBA = Mod_Mem_EmulatorViewModel.GBA;

        Games = new ObservableCollection<Mod_Mem_GameViewModel>()
        {
            new Mod_Mem_GameViewModel(
                game: new Mod_Mem_Ray1Game(Ray1EngineVersion.PC),
                displayName: new ResourceLocString(nameof(Resources.Mod_Mem_Game_R1_PC_1_21)),
                getOffsetsFunc: () => Mod_Mem_Ray1MemoryData.Offsets_PC_1_21,
                emulators: emuMSDOS),
            new Mod_Mem_GameViewModel(
                game: new Mod_Mem_Ray1Game(Ray1EngineVersion.PS1),
                displayName: new ResourceLocString(nameof(Resources.Mod_Mem_Game_R1_PS1_US)),
                getOffsetsFunc: () => Mod_Mem_Ray1MemoryData.Offsets_PS1_US,
                emulators: emuPS1),
            new Mod_Mem_GameViewModel(
                game: new Mod_Mem_Ray1Game(Ray1EngineVersion.R2_PS1),
                displayName: new ResourceLocString(nameof(Resources.Mod_Mem_Game_R2_PS1_Proto)),
                getOffsetsFunc: () => Mod_Mem_Ray1MemoryData.Offsets_PS1_R2,
                emulators: emuPS1),
            new Mod_Mem_GameViewModel(
                game: new Mod_Mem_Ray1Game(Ray1EngineVersion.GBA),
                displayName: new ResourceLocString(nameof(Resources.Mod_Mem_Game_R1_GBA_EU)),
                getOffsetsFunc: () => Mod_Mem_Ray1MemoryData.Offsets_GBA_EU,
                emulators: emuGBA),
            new Mod_Mem_GameViewModel(
                game: new Mod_Mem_CPAGame(new OpenSpaceSettings(EngineVersion.Rayman2, Platform.PC)),
                displayName: new ResourceLocString(nameof(Resources.Mod_Mem_Game_R2_PC)),
                getOffsetsFunc: () => Mod_Mem_CPAMemoryData.Offsets_R2_PC,
                emulators: emuNone,
                processNameKeywords: new [] { "Rayman2" }),
            new Mod_Mem_GameViewModel(
                game: new Mod_Mem_CPAGame(new OpenSpaceSettings(EngineVersion.Rayman3, Platform.PC)),
                displayName: new ResourceLocString(nameof(Resources.Mod_Mem_Game_R3_PC)),
                getOffsetsFunc: () => Mod_Mem_CPAMemoryData.Offsets_R3_PC,
                emulators: emuNone,
                processNameKeywords: new [] { "Rayman3" }),
        };
        SelectedGame = Games.First();

        ProcessAttacherViewModel.ProcessNameKeywords = Games.
            SelectMany(x => x.Emulators.SelectMany(e => e.ProcessNameKeywords).Concat(x.ProcessNameKeywords)).
            Distinct().
            ToArray();

        EditorFieldGroups = new ObservableCollection<EditorFieldGroupViewModel>();
        InfoItems = new ObservableCollection<DuoGridItemViewModel>();
        Actions = new ObservableCollection<Mod_Mem_ActionViewModel>();

        BindingOperations.EnableCollectionSynchronization(EditorFieldGroups, Application.Current);
        BindingOperations.EnableCollectionSynchronization(InfoItems, Application.Current);
        BindingOperations.EnableCollectionSynchronization(Actions, Application.Current);

        RefreshLogCommand = new RelayCommand(RefreshLog);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private Mod_Mem_UI? _uiContent;
    private CancellationTokenSource? _updateCancellation;
    private Mod_Mem_MemoryDataContainer? _memContainer;
    private Mod_Mem_GameViewModel _selectedGame;
    private bool _logNextTick;

    #endregion

    #region Services

    public IMessageUIManager MessageUI { get; }

    #endregion

    #region Commands

    public ICommand RefreshLogCommand { get; }

    #endregion

    #region Public Properties

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.Mod_Mem_Header));
    public override GenericIconKind Icon => GenericIconKind.Mods_Mem;
    public override object UIContent => _uiContent ??= new Mod_Mem_UI()
    {
        DataContext = this
    };

    public ProcessAttacherViewModel ProcessAttacherViewModel { get; }
    public Context? Context { get; private set; }

    public ObservableCollection<Mod_Mem_GameViewModel> Games { get; }
    public Mod_Mem_GameViewModel SelectedGame
    {
        get => _selectedGame;
        [MemberNotNull(nameof(SelectedEmulator), nameof(_selectedGame))]
        set
        {
            _selectedGame = value;
            Emulators = new ObservableCollection<Mod_Mem_EmulatorViewModel>(value.Emulators);
            SelectedEmulator = Emulators.First();
        }
    }
    public Mod_Mem_Game? AttachedGame { get; set; }

    public ObservableCollection<Mod_Mem_EmulatorViewModel>? Emulators { get; set; }
    public Mod_Mem_EmulatorViewModel SelectedEmulator { get; set; }

    public ObservableCollection<EditorFieldGroupViewModel> EditorFieldGroups { get; }
    public ObservableCollection<DuoGridItemViewModel> InfoItems { get; }
    public ObservableCollection<Mod_Mem_ActionViewModel> Actions { get; }
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

        foreach (Mod_Mem_GameViewModel game in Games)
        {
            if (game.ProcessNameKeywords.Concat(game.Emulators.SelectMany(x => x.ProcessNameKeywords)).
                Any(x => processName.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) != -1))
            {
                SelectedGame = game;
                return;
            }
        }
    }

    private async void AttachProcess(AttachableProcessViewModel p)
    {
        try
        {
            // Create a new context
            DisposeContext();
            
            AttachedGame?.DetachContainer();
            AttachedGame = SelectedGame.Game;
            
            Context = new RCPContext(String.Empty, log: new MemorySerializerLog()
            {
                IsEnabled = false, // Start disabled as we only want to log individual ticks
            });

            Mod_Mem_MemoryData memData = AttachedGame.CreateMemoryData(Context);
            _memContainer = new Mod_Mem_MemoryDataContainer(memData);

            AttachedGame.AttachContainer(_memContainer);
            AttachedGame.InitializeContext(Context);

            BinaryDeserializer s = Context.Deserializer;

            foreach (Mod_Mem_MemoryRegion memRegion in SelectedEmulator.MemoryRegions)
            {
                // Open the process as a stream
                ProcessMemoryStream stream = new(p.Process, ProcessMemoryStream.Mode.AllAccess);

                var file = Context.AddFile(new ProcessMemoryStreamFile(
                    context: Context, 
                    name: memRegion.Name, 
                    baseAddress: memRegion.GameOffset,
                    memoryRegionLength: memRegion.Length,
                    stream: new BufferedStream(stream), 
                    leaveOpen: true));

                // Initialize the memory stream
                s.Goto(file.StartPointer);
                InitializeProcessStream(stream, memRegion, s);
            }

            memData.Initialize(Context, SelectedGame.GetOffsets());

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

    private static void InitializeProcessStream(ProcessMemoryStream stream, Mod_Mem_MemoryRegion memRegion, BinaryDeserializer s)
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
        MemorySerializerLog? logger = Context?.Log as MemorySerializerLog;

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

        foreach (Mod_Mem_ActionViewModel action in Actions)
            action.Refresh();
    }

    #endregion

    #region Public Methods

    public override async Task InitializeAsync()
    {
        await ProcessAttacherViewModel.RefreshProcessesAsync();
    }

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

        BindingOperations.DisableCollectionSynchronization(EditorFieldGroups);
        BindingOperations.DisableCollectionSynchronization(InfoItems);
    }

    #endregion
}