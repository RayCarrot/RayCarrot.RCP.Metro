using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using NLog;

namespace RayCarrot.RCP.Metro;

// TODO-UPDATE: Log

public class ProcessAttacherViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    public ProcessAttacherViewModel()
    {
        Processes = new ObservableCollection<AttachableProcessViewModel>();

        BindingOperations.EnableCollectionSynchronization(Processes, App.Current);

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

    public event EventHandler<AttachableProcessEventArgs>? ProcessAttached;
    public event EventHandler<AttachableProcessEventArgs>? ProcessDetached;

    #endregion

    #region Public Properties

    public ObservableCollection<AttachableProcessViewModel> Processes { get; }
    public AttachableProcessViewModel? SelectedProcess { get; set; }
    public AttachableProcessViewModel? AttachedProcess { get; set; }
    public bool IsAttached => AttachedProcess != null;

    #endregion

    #region Private Methods

    private void ClearProcesses(bool includeSelected)
    {
        foreach (AttachableProcessViewModel p in Processes)
            if (includeSelected || p != SelectedProcess)
                p.Dispose();

        Processes.Clear();
    }

    #endregion

    #region Protected Methods

    protected virtual void OnProcessAttached(AttachableProcessEventArgs e) => ProcessAttached?.Invoke(this, e);
    protected virtual void OnProcessDetached(AttachableProcessEventArgs e) => ProcessDetached?.Invoke(this, e);

    #endregion

    #region Public Methods

    public Task RefreshProcessesAsync()
    {
        if (_refreshing)
            return Task.CompletedTask;

        _refreshing = true;

        try
        {
            ClearProcesses(true);

            return Task.Run(() =>
            {
                Process current = Process.GetCurrentProcess();

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

                        Processes.Add(new AttachableProcessViewModel(p, path));
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug(ex, "Getting process module");
                        p.Dispose();
                    }
                }

                // TODO-UPDATE: Auto select most likely process in list based on name
                SelectedProcess = Processes.FirstOrDefault();
            });
        }
        finally
        {
            _refreshing = false;
        }
    }

    public void AttachProcess()
    {
        if (SelectedProcess == null)
            return;

        AttachedProcess = SelectedProcess;
        OnProcessAttached(new AttachableProcessEventArgs(AttachedProcess));
        ClearProcesses(false);
    }

    public Task DetachProcessAsync()
    {
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
        BindingOperations.DisableCollectionSynchronization(Processes);
    }

    #endregion

}