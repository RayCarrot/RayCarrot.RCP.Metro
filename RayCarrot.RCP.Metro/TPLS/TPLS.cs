using RayCarrot.CarrotFramework.Abstractions;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    public class TPLS
    {
        #region Constructor

        static TPLS()
        {
            Data = new TPLSDataViewModel();
        }

        public TPLS()
        {
            RCFCore.Logger?.LogInformationSource("A new instance of TPLS has been started");

            BGM = new BGM(Data);
            MidiPlayer = new Midi(Data);
            PosBGM = new PositionedBGM(Data);
            Watchdog = new Watchdog(RCFRCP.Data.TPLSData.RaymanVersion, RCFRCP.Data.TPLSData.DosBoxVersion, BGM, MidiPlayer, PosBGM);
        }

        #endregion

        #region Private Properties

        private Watchdog Watchdog { get; }

        private BGM BGM { get; }

        private Midi MidiPlayer { get; }

        private PositionedBGM PosBGM { get; }

        #endregion

        #region Public Static Properties

        public static TPLSDataViewModel Data { get; }

        #endregion

        #region Private Static Properties

        private static CancellationTokenSource CancellationTokenSource { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts TPLS
        /// </summary>
        public void Start(Process dosBoxProcess)
        {
            if (Data.IsRunning)
            {
                RCFCore.Logger?.LogInformationSource("TPLS: Is already running");
                return;
            }

            Data.IsRunning = true;

            CancellationTokenSource = new CancellationTokenSource();

            // Create a new thread
            Task task = new Task(async () =>
            {
                try
                {
                    await Watchdog.StartWatchingRaymanAsync(dosBoxProcess, CancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    ex.HandleError("TPLS watchdog");
                }
                finally
                {
                    Watchdog?.Dispose();
                    Data.IsRunning = false;
                }
            });

            // Subscribe to events
            Watchdog.XAxisChanged += XAxisChangedAsync;
            Watchdog.YAxisChanged += YAxisChangedAsync;
            Watchdog.WorldChanged += WorldChanged;
            Watchdog.LevelChanged += LevelChanged;
            Watchdog.BossEventChanged += BossEventChanged;
            Watchdog.MusicOnOffChanged += MusicOnOffChangedAsync;
            Watchdog.OptionsOffChanged += OptionsOffChangedAsync;
            Watchdog.OptionsOnChanged += OptionsOnChanged;
            Watchdog.RaymanInLevelChanged += RaymanInLevelChangedAsync;
            Watchdog.VolumeChanged += VolumeChanged;

            // Start the thread
            task.Start();
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Stops a currently running TPLS service
        /// </summary>
        public static void StopCurrent()
        {
            CancellationTokenSource?.Cancel();
        }

        #endregion

        #region Event Handlers

        private async void XAxisChangedAsync(object sender, ValueEventArgs<short> e)
        {
            Data.XAxis = e.Value;
            await PosBGM.AxisChangeAsync();
        }

        private async void YAxisChangedAsync(object sender, ValueEventArgs<short> e)
        {
            Data.YAxis = e.Value;
            await PosBGM.AxisChangeAsync();
        }

        private void WorldChanged(object sender, ValueEventArgs<string> e)
        {
            Data.World = e.Value;
        }

        private void LevelChanged(object sender, ValueEventArgs<string> e)
        {
            Data.Level = e.Value;
        }

        private void BossEventChanged(object sender, ValueEventArgs<bool> e)
        {
            Data.BossEvent = e.Value;
            BGM.BossEventChanged();
            PosBGM.BossEventChanged();
        }

        private async void MusicOnOffChangedAsync(object sender, ValueEventArgs<bool> e)
        {
            Data.Music = e.Value;
            await BGM.OnMusicOnOffChangeAsync();
            await MidiPlayer.OnMusicOnOffChangeAsync();
            await PosBGM.OnMusicOnOffChangeAsync();
        }

        private async void OptionsOffChangedAsync(object sender, ValueEventArgs<bool> e)
        {
            Data.OptionsOff = e.Value;
            await BGM.OnOptionsOffChangeAsync();
            await MidiPlayer.OnOptionsOffChangeAsync();
            await PosBGM.OnOptionsOffChangeAsync();
        }

        private void OptionsOnChanged(object sender, ValueEventArgs<bool> e)
        {
            Data.OptionsOn = e.Value;
            BGM.OnOptionnsOnChange();
            MidiPlayer.OnOptionnsOnChange();
            PosBGM.OnOptionnsOnChange();
        }

        private async void RaymanInLevelChangedAsync(object sender, ValueEventArgs<bool> e)
        {
            Data.InLevel = e.Value;
            await BGM.OnRaymanInLevelChangeAsync();
            await MidiPlayer.OnRaymanInLevelChangeAsync();
            await PosBGM.OnRaymanInLevelChangeAsync();
        }

        private void VolumeChanged(object sender, ValueEventArgs<float> e)
        {
            BGM.Player.VolumeContrl(e.Value);
            MidiPlayer.Player.VolumeContrl(e.Value);
            PosBGM.Player.VolumeContrl(e.Value);
        }

        #endregion
    }
}