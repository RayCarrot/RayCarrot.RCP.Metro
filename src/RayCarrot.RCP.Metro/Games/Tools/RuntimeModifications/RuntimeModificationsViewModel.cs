using System.Diagnostics;
using System.Windows.Input;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro.Games.Tools.RuntimeModifications;

// TODO-UPDATE: Check for old loc messages mentioning memory loading. Test everything. Test failures too.
public class RuntimeModificationsViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    public RuntimeModificationsViewModel(GameInstallation gameInstallation, IMessageUIManager messageUi)
    {
        // Set properties
        GameInstallation = gameInstallation ?? throw new ArgumentNullException(nameof(gameInstallation));
        MessageUI = messageUi ?? throw new ArgumentNullException(nameof(messageUi));

        AvailableProcesses = new ObservableCollection<ProcessViewModel>();
        AvailableProcesses.EnableCollectionSynchronization();

        // Get the available game versions from components. Some games have different versions, such as Rayman 1 on PC,
        // or different regional releases, such as most console games, where memory offsets will differ.
        AvailableGames = new ObservableCollection<GameViewModel>(GameInstallation.
            GetComponents<RuntimeModificationsGameManagersComponent>().
            CreateManyObjects().
            Select(x => new GameViewModel(x)));
        SelectedGame = AvailableGames.First();

        // Get the available emulator versions. We could have these be registered as components, but then we won't get
        // any emulator versions if the game is launched through a non-emulator client such as Ubisoft Connect.
        EmulatorManager[] emulatorVersions = GameInstallation.GetRequiredComponent<RuntimeModificationsGameManagersComponent>().EmulatedPlatform switch
        {
            EmulatedPlatform.None => EmulatorManager.None,
            EmulatedPlatform.MsDos => EmulatorManager.MsDos,
            EmulatedPlatform.Gba => EmulatorManager.Gba,
            EmulatedPlatform.Ps1 => EmulatorManager.Ps1,
            _ => throw new ArgumentOutOfRangeException()
        };
        AvailableEmulators = new ObservableCollection<EmulatorViewModel>(emulatorVersions.Select(x => new EmulatorViewModel(x)));
        SelectedEmulator = AvailableEmulators.First();

        using (Process p = Process.GetCurrentProcess())
            CurrentProcessId = p.Id;
        SwitchToAutoFindGame();

        // Create commands
        SwitchToManuallyFindGameCommand = new RelayCommand(SwitchToManuallyFindGame);
        SwitchToAutoFindGameCommand = new RelayCommand(SwitchToAutoFindGame);
        RefreshAvailableProcessesCommand = new AsyncRelayCommand(RefreshAvailableProcessesAsync);
        AttachSelectedProcessCommand = new AsyncRelayCommand(AttachSelectedProcessAsync);
        DetachProcessCommand = new RelayCommand(DetachProcess);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Services

    public IMessageUIManager MessageUI { get; }

    #endregion

    #region Private Properties

    private bool IsRefreshingAvailableProcesses { get; set; }
    private int CurrentProcessId { get; }
    private CancellationTokenSource? AutoFindGameCancellation { get; set; }
    private CancellationTokenSource? RefreshGameFieldsCancellation { get; set; }

    #endregion

    #region Commands

    public ICommand SwitchToManuallyFindGameCommand { get; }
    public ICommand SwitchToAutoFindGameCommand { get; }
    public ICommand RefreshAvailableProcessesCommand { get; }
    public ICommand AttachSelectedProcessCommand { get; }
    public ICommand DetachProcessCommand { get; }

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }
    public ViewModelState State { get; set; }

    public ObservableCollection<ProcessViewModel> AvailableProcesses { get; }
    public ProcessViewModel? SelectedProcess { get; set; }

    public ObservableCollection<GameViewModel> AvailableGames { get; }
    public GameViewModel SelectedGame { get; set; }

    public ObservableCollection<EmulatorViewModel> AvailableEmulators { get; }
    public EmulatorViewModel SelectedEmulator { get; set; }

    public RunningGameViewModel? RunningGameViewModel { get; set; }

    #endregion

    #region Private Methods

    private IEnumerable<Process> GetProcesses()
    {
        foreach (Process p in Process.GetProcesses())
        {
            try
            {
                // Don't include ourselves
                if (p.Id == CurrentProcessId)
                {
                    p.Dispose();
                    continue;
                }

                // Make sure there is a main window
                if (p.MainWindowHandle == IntPtr.Zero)
                {
                    p.Dispose();
                    continue;
                }

                // Get the main module. This might fail!
                _ = p.MainModule;
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Checking process");
                p.Dispose();
                continue;
            }
            
            yield return p;
        }
    }

    private void ClearAvailableProcesses(bool includeSelected)
    {
        Logger.Info("Clearing available processes list");

        foreach (ProcessViewModel p in AvailableProcesses)
            if (includeSelected || p != SelectedProcess)
                p.Process.Dispose();

        AvailableProcesses.Clear();
    }

    private void SearchForRunningGame()
    {
        // Get the currently available processes
        foreach (Process process in GetProcesses())
        {
            foreach (EmulatorViewModel emulator in AvailableEmulators)
            {
                try
                {
                    if (emulator.EmulatorManager.IsProcessValid(process))
                    {
                        foreach (GameViewModel game in AvailableGames)
                        {
                            // TODO-UPDATE: Check in game manager if process is valid

                            RunningGameViewModel runningGame = new(game.GameManager, emulator.EmulatorManager, process);
                            // TODO-UPDATE: Check in game manager if game memory is valid
                            runningGame.RefreshFields();

                            SwitchToFoundGame(runningGame);
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // TODO-UPDATE: Handle
                }
            }
        }
    }

    #endregion

    #region Public Methods

    public void SwitchToAutoFindGame()
    {
        State = ViewModelState.AutoFindGame;

        AutoFindGameCancellation?.Cancel();
        AutoFindGameCancellation?.Dispose();
        AutoFindGameCancellation = null;

        AutoFindGameCancellation = new CancellationTokenSource();
        CancellationToken token = AutoFindGameCancellation.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                try
                {
                    Logger.Info("Entered search for running game loop");

                    while (!token.IsCancellationRequested)
                    {
                        SearchForRunningGame();

                        // Wait 1 second
                        await Task.Delay(TimeSpan.FromSeconds(1), token);
                    }
                }
                finally
                {
                    Logger.Info("Exited search for running game loop");
                }
            }
            catch (OperationCanceledException ex)
            {
                Logger.Debug(ex, "Auto finding game");
            }
            catch (Exception ex)
            {
                // We should never end up here, but let's log it anyway to avoid the exception being swallowed
                Logger.Error(ex, "Auto finding game");
            }
        });
    }

    public async void SwitchToManuallyFindGame()
    {
        State = ViewModelState.ManuallyFindGame;
        AutoFindGameCancellation?.Cancel();
        await RefreshAvailableProcessesAsync();
    }

    public void SwitchToFoundGame(RunningGameViewModel viewModel)
    {
        State = ViewModelState.FoundGame;
        AutoFindGameCancellation?.Cancel();

        RunningGameViewModel?.Dispose();
        RunningGameViewModel = viewModel;

        // Create a cancellation source
        RefreshGameFieldsCancellation?.Cancel();
        RefreshGameFieldsCancellation?.Dispose();
        RefreshGameFieldsCancellation = new CancellationTokenSource();
        CancellationToken token = RefreshGameFieldsCancellation.Token;

        // IDEA: Use DispatcherTimer instead? That will cause the tick to occur on the UI thread which solves some threading issues
        //       and removes the need to use locks. However we get slightly less control over how and when the ticks occur and if
        //       the code is too slow it will impact the UI.

        // Start refreshing
        _ = Task.Run(async () =>
        {
            try
            {
                try
                {
                    Logger.Info("Entered refresh fields loop");

                    while (!token.IsCancellationRequested)
                    {
                        RunningGameViewModel.RefreshFields();

                        // The games only update 60 frames per second, so we do the same
                        await Task.Delay(TimeSpan.FromSeconds(1 / 60d), token);
                    }
                }
                finally
                {
                    Logger.Info("Exited refresh fields loop");
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

                if (RunningGameViewModel.ProcessViewModel.Process.HasExited)
                {
                    Logger.Debug(ex, "Updating memory mod fields");
                }
                else
                {
                    Logger.Warn(ex, "Updating memory mod fields");

                    await MessageUI.DisplayMessageAsync(Resources.Mod_Mem_TickError, MessageType.Error);
                }
            }

            // We could switch to auto find game, but we might enter an endless loop of error messages then
            SwitchToManuallyFindGame();

            RunningGameViewModel?.Dispose();
            RefreshGameFieldsCancellation?.Dispose();
            RunningGameViewModel = null;
            RefreshGameFieldsCancellation = null;
        });
    }

    public void DetachProcess()
    {
        RefreshGameFieldsCancellation?.Cancel();
    }

    public async Task RefreshAvailableProcessesAsync()
    {
        if (IsRefreshingAvailableProcesses)
            return;

        IsRefreshingAvailableProcesses = true;

        try
        {
            ClearAvailableProcesses(true);
            SelectedProcess = null;

            Logger.Info("Refreshing processes list");

            await Task.Run(() =>
            {
                foreach (Process p in GetProcesses())
                {
                    ProcessViewModel vm = new(p, ShellThumbnailSize.Small);

                    SelectedProcess ??= vm;
                    AvailableProcesses.Add(vm);
                }
            });
        }
        finally
        {
            IsRefreshingAvailableProcesses = false;
        }
    }

    public async Task AttachSelectedProcessAsync()
    {
        if (SelectedProcess == null)
            return;

        Logger.Info("Attaching to process {0}", SelectedProcess.ProcessName);

        ClearAvailableProcesses(false);

        RunningGameViewModel runningGame;
        try
        {
            runningGame = new RunningGameViewModel(SelectedGame.GameManager, SelectedEmulator.EmulatorManager, SelectedProcess.Process);
            runningGame.RefreshFields();
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Attaching to selected process");

            await MessageUI.DisplayMessageAsync(Resources.Mod_Mem_AttachError, MessageType.Error);

            SwitchToManuallyFindGame();
            return;
        }

        SwitchToFoundGame(runningGame);
    }

    public virtual void Dispose()
    {
        RefreshGameFieldsCancellation?.Cancel();
        RefreshGameFieldsCancellation?.Dispose();
        AutoFindGameCancellation?.Cancel();
        AutoFindGameCancellation?.Dispose();
        ClearAvailableProcesses(true);
    }

    #endregion

    #region Data Types

    public enum ViewModelState
    {
        AutoFindGame,
        ManuallyFindGame,
        FoundGame,
    }

    #endregion
}