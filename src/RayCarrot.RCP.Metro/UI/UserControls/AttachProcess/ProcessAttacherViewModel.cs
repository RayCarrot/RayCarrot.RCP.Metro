﻿using System.Diagnostics;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

public class ProcessAttacherViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    public ProcessAttacherViewModel()
    {
        Processes = new ObservableCollection<AttachableProcessViewModel>();

        Processes.EnableCollectionSynchronization();

        RefreshProcessesCommand = new AsyncRelayCommand(RefreshProcessesAsync);
        AttachProcessCommand = new RelayCommand(AttachProcess);
        DetachProcessCommand = new AsyncRelayCommand(DetachProcessAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private bool _refreshing;

    #endregion

    #region Commands

    public ICommand RefreshProcessesCommand { get; }
    public ICommand AttachProcessCommand { get; }
    public ICommand DetachProcessCommand { get; }

    #endregion

    #region Events

    public event EventHandler? Refreshed;
    public event EventHandler<AttachableProcessEventArgs>? ProcessAttached;
    public event EventHandler<AttachableProcessEventArgs>? ProcessDetached;

    #endregion

    #region Public Properties

    public string[]? ProcessNameKeywords { get; set; }
    public ObservableCollection<AttachableProcessViewModel> Processes { get; }
    public AttachableProcessViewModel? SelectedProcess { get; set; }
    public AttachableProcessViewModel? AttachedProcess { get; set; }
    public bool IsAttached => AttachedProcess != null;

    #endregion

    #region Private Methods

    private void ClearProcesses(bool includeSelected)
    {
        Logger.Info("Clearing processes list");

        foreach (AttachableProcessViewModel p in Processes)
            if (includeSelected || p != SelectedProcess)
                p.Dispose();

        Processes.Clear();
    }

    #endregion

    #region Protected Methods

    protected virtual void OnRefreshed() => Refreshed?.Invoke(this, EventArgs.Empty);
    protected virtual void OnProcessAttached(AttachableProcessEventArgs e) => ProcessAttached?.Invoke(this, e);
    protected virtual void OnProcessDetached(AttachableProcessEventArgs e) => ProcessDetached?.Invoke(this, e);

    #endregion

    #region Public Methods

    public async Task RefreshProcessesAsync()
    {
        if (_refreshing)
            return;

        _refreshing = true;

        try
        {
            ClearProcesses(true);

            Logger.Info("Refreshing processes list");

            await Task.Run(() =>
            {
                Process current = Process.GetCurrentProcess();

                bool foundMatch = false;

                foreach (Process p in Process.GetProcesses())
                {
                    try
                    {
                        // Don't include ourselves
                        if (p.Id == current.Id)
                            continue;

                        // Make sure there is a main window
                        if (p.MainWindowHandle == IntPtr.Zero)
                        {
                            p.Dispose();
                            continue;
                        }

                        // Get the main module. This might fail!
                        ProcessModule? module = p.MainModule;

                        // Get the file path
                        FileSystemPath path = module?.FileName;

                        // Make sure the path exists
                        if (!path.FileExists)
                        {
                            p.Dispose();
                            continue;
                        }

                        AttachableProcessViewModel vm = new(p, path);

                        SelectedProcess ??= vm;

                        if (!foundMatch && 
                            ProcessNameKeywords != null && 
                            ProcessNameKeywords.Any(x => vm.ProcessName.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) != -1))
                        {
                            SelectedProcess = vm;
                            foundMatch = true;
                        }

                        Processes.Add(vm);
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug(ex, "Getting process module");
                        p.Dispose();
                    }
                }
            });
        }
        finally
        {
            _refreshing = false;
            OnRefreshed();
        }
    }

    public void AttachProcess()
    {
        if (SelectedProcess == null)
            return;

        Logger.Info("Attaching to process {0}", SelectedProcess.ProcessName);

        AttachedProcess = SelectedProcess;
        ClearProcesses(false);
        OnProcessAttached(new AttachableProcessEventArgs(AttachedProcess));
    }

    public Task DetachProcessAsync()
    {
        Logger.Info("Detaching from process");

        AttachableProcessViewModel? attachedProcess = AttachedProcess;
        AttachedProcess = null;

        if (attachedProcess != null)
        {
            OnProcessDetached(new AttachableProcessEventArgs(attachedProcess));
            attachedProcess.Dispose();
        }

        return RefreshProcessesAsync();
    }

    public void Dispose()
    {
        ClearProcesses(true);
        AttachedProcess?.Dispose();
    }

    #endregion
}