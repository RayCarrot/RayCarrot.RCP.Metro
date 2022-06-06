﻿using System;
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

    protected Mod_ProcessEditorViewModel()
    {
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
    protected virtual string? ModuleName => null;
    protected abstract long GameBaseOffset { get; }
    protected abstract bool IsGameBaseAPointer { get; }

    #endregion

    #region Public Properties

    public ProcessAttacherViewModel ProcessAttacherViewModel { get; }
    public Context? Context { get; private set; }

    #endregion

    #region Private Methods

    private void AttachProcess(AttachableProcessViewModel p)
    {
        // Open the process as a stream
        ProcessMemoryStream stream = new(p.Process, ProcessMemoryStream.Mode.AllAccess); // TODO-UPDATE: This might fail

        // Create a new context
        Context?.Dispose();
        Context = new RCPContext(String.Empty, noLog: true);
        InitializeContext(Context);

        StreamFile file = Context.AddFile(new ProcessMemoryStreamFile(Context, p.ProcessName, new BufferedStream(stream), leaveOpen: true));
        BinaryDeserializer s = Context.Deserializer;

        // Initialize the memory stream
        s.Goto(file.StartPointer);
        InitializeProcessStream(stream, s); // TODO-UPDATE: This might fail

        // Initialize the fields
        InitializeFields(file.StartPointer);

        // Create a cancellation source
        _updateCancellation = new CancellationTokenSource();
        CancellationToken token = _updateCancellation.Token;

        // Start refreshing
        Task.Run(async () =>
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
                if (ProcessAttacherViewModel.AttachedProcess?.Process.HasExited == true)
                {
                    Logger.Debug(ex, "Updating memory mod fields");
                }
                else
                {
                    Logger.Warn(ex, "Updating memory mod fields");
                    // TODO: Error message
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

    private void InitializeProcessStream(ProcessMemoryStream stream, BinaryDeserializer s)
    {
        string? moduleName = ModuleName;
        long processBase = (moduleName == null
            ? stream.Process.MainModule
            : stream.Process.Modules.Cast<ProcessModule>().First(x => x.ModuleName == moduleName)).BaseAddress.ToInt64();

        long baseStreamOffset;

        if (IsGameBaseAPointer)
        {
            Pointer basePtrPtr = s.CurrentPointer + GameBaseOffset;

            // Get the base pointer
            baseStreamOffset = stream.Is64Bit 
                ? s.DoAt(basePtrPtr, () => s.Serialize<long>(default))
                : s.DoAt(basePtrPtr, () => s.Serialize<uint>(default));
        }
        else
        {
            baseStreamOffset = GameBaseOffset + processBase;
        }

        stream.BaseStreamOffset = baseStreamOffset;
    }

    #endregion

    #region Protected Method

    protected virtual void InitializeContext(Context context) { }
    protected virtual void InitializeFields(Pointer offset) { }
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
        Context?.Dispose();
    }

    #endregion
}

public abstract class Mod_ProcessEditorViewModel<TMemObj> : Mod_ProcessEditorViewModel
    where TMemObj : Mod_MemoryData, new()
{
    #region Private Fields

    private TMemObj? _memData;
    private readonly object _lock = new();

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

    protected override void InitializeFields(Pointer offset)
    {
        base.InitializeFields(offset);

        lock (_lock)
            _memData = new TMemObj()
            {
                Offset = offset
            };
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

        if (Context == null || _memData == null)
            return;

        // Serialize the data. Depending on if a value has changed
        // or not this will either read or write the data.
        lock (_lock)
            _memData.Serialize(Context);
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