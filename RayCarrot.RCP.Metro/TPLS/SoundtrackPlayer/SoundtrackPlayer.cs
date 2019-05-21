using NAudio.Wave;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A base soundtrack player for TPLS
    /// </summary>
    public abstract class SoundtrackPlayer
    {
        #region Constructor

        protected SoundtrackPlayer(TPLSDataViewModel data)
        {
            Data = data;
            Player = new Player();
        }

        #endregion

        #region Public Properties

        public Player Player { get; }

        #endregion

        #region Protected Properties

        protected TPLSDataViewModel Data { get; }

        #endregion

        protected abstract Task PlayAsync();

        protected abstract long[][] Soundtrack { get; }

        public async Task OnMusicOnOffChangeAsync()
        {
            if (Data.InLevel && Soundtrack != null)
            {
                if (!Data.Music && !Data.OptionsOff && Data.OptionsOn && (Player.PlaybackState == PlaybackState.Paused))
                    Stop();
                else if (Data.Music && Data.OptionsOff && !Data.OptionsOn && Player.PlaybackState == PlaybackState.Paused)
                    Player.Resume();
                else if (Data.Music && Data.OptionsOff && !Data.OptionsOn && Player.PlaybackState == PlaybackState.Stopped)
                    await PlayAsync();

            }
        }

        public async Task OnOptionsOffChangeAsync()
        {
            if (Data.OptionsOff && !Data.Music)
                // ReSharper disable once RedundantJumpStatement
                return;
            else if (Data.OptionsOff && Player.PlaybackState == PlaybackState.Paused)
                Player.Resume();
            else if (Data.Music && Data.OptionsOff && !Data.OptionsOn && Player.PlaybackState == PlaybackState.Stopped)
                await PlayAsync();
        }

        public void OnOptionnsOnChange()
        {
            if (Data.OptionsOn)
                Player.Pause();
        }

        public async Task OnRaymanInLevelChangeAsync()
        {
            if (Data.Music && Data.OptionsOff && !Data.OptionsOn && Data.InLevel && Soundtrack != null)
                await PlayAsync();
            else
                Fade(1, 1000);
        }

        public void BossEventChanged()
        {
            if (Data.BossEvent)
                Fade(1, 1000);
        }

        private void Stop()
        {
            if (Player.PlaybackState == PlaybackState.Playing || Player.PlaybackState == PlaybackState.Paused)
                Player.Stop();
        }

        private void Fade(int delay, int length)
        {
            if (Player.PlaybackState == PlaybackState.Playing || Player.PlaybackState == PlaybackState.Paused)
                Player.Fade(delay, length);
        }
    }
}
