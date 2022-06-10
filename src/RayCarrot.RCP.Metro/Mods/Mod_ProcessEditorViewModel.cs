using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinarySerializer;
using NLog;

namespace RayCarrot.RCP.Metro;

public abstract class Mod_ProcessEditorViewModel : Mod_BaseViewModel, IDisposable
{
    #region Constructor

    protected Mod_ProcessEditorViewModel(IMessageUIManager messageUi)
    {
        MessageUI = messageUi ?? throw new ArgumentNullException(nameof(messageUi));

        ProcessAttacherViewModel = new ProcessAttacherViewModel();
        ProcessAttacherViewModel.ProcessAttached += (_, e) => AttachProcess(e.AttachedProcess);
        ProcessAttacherViewModel.ProcessDetached += (_, _) => DetachProcess();
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private CancellationTokenSource? _updateCancellation;

    #endregion

    #region Protected Properties

    protected virtual string[]? ProcessNameKeywords => null;
    protected abstract Mod_MemoryRegion[] MemoryRegions { get; }

    #endregion

    #region Services

    public IMessageUIManager MessageUI { get; }

    #endregion

    #region Public Properties

    public ProcessAttacherViewModel ProcessAttacherViewModel { get; }
    public Context? Context { get; private set; }

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
            Context = new RCPContext(String.Empty, noLog: true);
            InitializeContext(Context);
            
            BinaryDeserializer s = Context.Deserializer;

            foreach (Mod_MemoryRegion memRegion in MemoryRegions)
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

            // Initialize the fields
            InitializeFields();
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
        ClearFields();
    }

    private static void InitializeProcessStream(ProcessMemoryStream stream, Mod_MemoryRegion memRegion, BinaryDeserializer s)
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

    #endregion

    #region Protected Method

    protected virtual void InitializeContext(Context context) { }
    protected virtual void InitializeFields() { }
    protected virtual void ClearFields() { }
    protected virtual void RefreshFields() { }

    #endregion

    #region Public Methods

    public override async Task InitializeAsync()
    {
        ProcessAttacherViewModel.ProcessNameKeywords = ProcessNameKeywords;
        await ProcessAttacherViewModel.RefreshProcessesAsync();
    }

    public virtual void Dispose()
    {
        _updateCancellation?.Cancel();
        _updateCancellation?.Dispose();
        ProcessAttacherViewModel.Dispose();
        DisposeContext();
    }

    #endregion
}

public abstract class Mod_ProcessEditorViewModel<TMemObj> : Mod_ProcessEditorViewModel
    where TMemObj : Mod_MemoryData, new()
{
    #region Constructor

    protected Mod_ProcessEditorViewModel(IMessageUIManager messageUi) : base(messageUi) { }

    #endregion

    #region Private Fields

    private TMemObj? _memData;
    private readonly object _lock = new();

    #endregion

    #region Protected Properties

    protected virtual Dictionary<string, long>? Offsets => null;

    #endregion

    #region Protected Methods

    protected T? AccessMemory<T>(Func<TMemObj, T> func)
    {
        if (_memData == null)
            return default;

        lock (_lock)
            return func(_memData);
    }

    protected void AccessMemory(Action<TMemObj> action)
    {
        if (_memData == null)
            return;

        lock (_lock)
            action(_memData);
    }

    protected override void InitializeFields()
    {
        base.InitializeFields();

        lock (_lock)
        {
            _memData = new TMemObj();
            _memData.Initialize(Context!, Offsets);
        }
    }

    protected override void ClearFields()
    {
        base.ClearFields();

        lock (_lock)
            _memData = null;
    }

    protected override void RefreshFields()
    {
        base.RefreshFields();

        if (_memData == null)
            return;

        // Serialize the data. Depending on if a value has changed
        // or not this will either read or write the data.
        lock (_lock)
            _memData.Serialize();
    }

    #endregion

    #region Public Methods

    public override void Dispose()
    {
        base.Dispose();
        _memData = null;
    }

    #endregion
}