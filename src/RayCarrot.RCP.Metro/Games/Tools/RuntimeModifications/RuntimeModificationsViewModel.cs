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
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private CancellationTokenSource? _updateCancellation;

    #endregion

    #region Services

    public IMessageUIManager MessageUI { get; }

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }
    public ProcessAttacherViewModel ProcessAttacherViewModel { get; }

    public ObservableCollection<GameVersionViewModel> GameVersions { get; }
    public GameVersionViewModel SelectedGameVersion { get; set; }

    public ObservableCollection<EmulatorVersionViewModel>? EmulatorVersions { get; set; }
    public EmulatorVersionViewModel SelectedEmulatorVersion { get; set; }

    public RunningGameViewModel? RunningGameViewModel { get; set; }

    #endregion

    #region Private Methods

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
        RunningGameViewModel?.Dispose();

        try
        {
            RunningGameViewModel = new RunningGameViewModel(SelectedGameVersion.Manager, SelectedEmulatorVersion.EmulatorVersion, p.Process);
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
                    RunningGameViewModel.RefreshFields();

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
        RunningGameViewModel?.Dispose();
        RunningGameViewModel = null;
    }

    #endregion

    #region Public Methods

    // TODO-UPDATE: Re-implement
    //public override async Task InitializeAsync()
    //{
    //    await ProcessAttacherViewModel.RefreshProcessesAsync();
    //}

    public virtual void Dispose()
    {
        _updateCancellation?.Cancel();
        _updateCancellation?.Dispose();
        ProcessAttacherViewModel.Dispose();
        RunningGameViewModel?.Dispose();
        RunningGameViewModel = null;
    }

    #endregion
}