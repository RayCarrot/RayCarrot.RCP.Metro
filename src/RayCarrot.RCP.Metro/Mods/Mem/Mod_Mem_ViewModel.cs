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
using BinarySerializer;
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
        ProcessAttacherViewModel.ProcessAttached += (_, e) => AttachProcess(e.AttachedProcess);
        ProcessAttacherViewModel.ProcessDetached += (_, _) => DetachProcess();

        Mod_Mem_EmulatorViewModel[] emuMSDOS = Mod_Mem_EmulatorViewModel.MSDOS;
        Mod_Mem_EmulatorViewModel[] emuPS1 = Mod_Mem_EmulatorViewModel.PS1;
        Mod_Mem_EmulatorViewModel[] emuGBA = Mod_Mem_EmulatorViewModel.GBA;

        // TODO-UPDATE: Localize
        Games = new ObservableCollection<Mod_Mem_GameViewModel>()
        {
            new Mod_Mem_GameViewModel(
                game: new Mod_Mem_R1Game(Ray1EngineVersion.PC),
                displayName: "Rayman 1 (PC - 1.21)",
                getOffsetsFunc: () => Mod_Mem_R1MemoryData.Offsets_PC_1_21,
                emulators: emuMSDOS),
            new Mod_Mem_GameViewModel(
                game: new Mod_Mem_R1Game(Ray1EngineVersion.PS1),
                displayName: "Rayman 1 (PS1 - US)",
                getOffsetsFunc: () => Mod_Mem_R1MemoryData.Offsets_PS1_US,
                emulators: emuPS1),
            new Mod_Mem_GameViewModel(
                game: new Mod_Mem_R1Game(Ray1EngineVersion.R2_PS1),
                displayName: "Rayman 2 (PS1 - Prototype)",
                getOffsetsFunc: () => Mod_Mem_R1MemoryData.Offsets_PS1_R2,
                emulators: emuPS1),
            new Mod_Mem_GameViewModel(
                game: new Mod_Mem_R1Game(Ray1EngineVersion.GBA),
                displayName: "Rayman Advance (GBA - EU)",
                getOffsetsFunc: () => Mod_Mem_R1MemoryData.Offsets_GBA_EU,
                emulators: emuGBA),
        };
        SelectedGame = Games.First();

        ProcessAttacherViewModel.ProcessNameKeywords = Games.
            SelectMany(x => x.Emulators).
            SelectMany(x => x.ProcessNameKeywords).
            Distinct().
            ToArray();

        EditorFieldGroups = new ObservableCollection<EditorFieldGroupViewModel>();
        InfoItems = new ObservableCollection<DuoGridItemViewModel>();
        Actions = new ObservableCollection<Mod_Mem_ActionViewModel>();

        BindingOperations.EnableCollectionSynchronization(EditorFieldGroups, Application.Current);
        BindingOperations.EnableCollectionSynchronization(InfoItems, Application.Current);
        BindingOperations.EnableCollectionSynchronization(Actions, Application.Current);
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

    #endregion

    #region Services

    public IMessageUIManager MessageUI { get; }

    #endregion

    #region Public Properties

    public override LocalizedString Header => "Memory Mods"; // TODO-UPDATE: Localize
    public override GenericIconKind Icon => GenericIconKind.Games; // TODO-UPDATE: Different icon
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

    private async void AttachProcess(AttachableProcessViewModel p)
    {
        try
        {
            // Create a new context
            DisposeContext();
            
            AttachedGame?.DetachContainer();
            AttachedGame = SelectedGame.Game;
            
            Context = new RCPContext(String.Empty, noLog: true);

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

            // TODO-UPDATE: Localize
            await MessageUI.DisplayMessageAsync("An error occurred when attaching to the process", "Error", MessageType.Error);

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
                await Task.Delay(TimeSpan.FromMilliseconds(400));

                if (ProcessAttacherViewModel.AttachedProcess?.Process.HasExited == true)
                {
                    Logger.Debug(ex, "Updating memory mod fields");
                }
                else
                {
                    Logger.Warn(ex, "Updating memory mod fields");

                    // TODO-UPDATE: Localize
                    await MessageUI.DisplayMessageAsync("An error occurred when updating the game values", "Error", MessageType.Error);
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
    }

    private static void InitializeProcessStream(ProcessMemoryStream stream, Mod_Mem_MemoryRegion memRegion, BinaryDeserializer s)
    {
        long processBase = (memRegion.ModuleName == null
            ? stream.Process.MainModule
            : stream.Process.Modules.Cast<ProcessModule>().First(x => x.ModuleName == memRegion.ModuleName)).BaseAddress.ToInt64();

        long baseStreamOffset;

        if (memRegion.IsProcessOffsetAPointer)
        {
            Pointer basePtrPtr = s.CurrentPointer + memRegion.ProcessOffset + processBase;

            // Get the base pointer
            baseStreamOffset = stream.Is64Bit 
                ? s.DoAt(basePtrPtr, () => s.Serialize<long>(default))
                : s.DoAt(basePtrPtr, () => s.Serialize<uint>(default));
        }
        else
        {
            baseStreamOffset = memRegion.ProcessOffset + processBase;
        }

        stream.BaseStreamOffset = baseStreamOffset;
    }

    private void RefreshFields()
    {
        _memContainer?.Update();

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