using NAudio.Vorbis;
using NAudio.Wave;
using RayCarrot.CarrotFramework.Abstractions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    public class Player
    {
        #region Public Methods

        public async Task PlayLoopAsync(string localData, long[] offset, long[] length)
        {
            try
            {
                if (PlaybackState == PlaybackState.Stopped)
                {
                    WaveOut = new WaveOutEvent();
                    loopStream = new LoopStream(
                        new VorbisWaveReader(new FileResolver(localData, offset[0], length[0])),
                        new VorbisWaveReader(new FileResolver(localData, offset[offset.Length - 1], length[length.Length - 1])));

                    fader = new DelayFadeOutSampleProvider(loopStream.ToSampleProvider());
                    WaveOut.Init(fader);
                    WaveOut.Play();
                }
            }
            catch (FileNotFoundException ex)
            {
                ex.HandleError("TPLS: Playing music");

                await RCFUI.MessageUI.DisplayMessageAsync(Resources.TPLS_PlaybackError_FileNotFound, Resources.TPLS_PlaybackErrorHeader, MessageType.Error);
            }
            catch (InvalidDataException ex)
            {
                ex.HandleError("TPLS: Playing music");

                await RCFUI.MessageUI.DisplayMessageAsync(Resources.TPLS_PlaybackError_InvalidData, Resources.TPLS_PlaybackErrorHeader, MessageType.Error);
            }
            catch (Exception ex)
            {
                ex.HandleError("TPLS: Playing music");

                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.TPLS_PlaybackError, Resources.TPLS_PlaybackErrorHeader);
            }
        }

        public void Fade(double delay, double duration)
        {
            if (fader == null || Fading)
                return;

            Fading = true;
            fader.BeginFadeOut(delay, duration);
            Thread FadeWaitOut = new Thread(() =>
            {
                Thread.Sleep((int)(delay + duration));
                Fading = false;
                Stop();
            });
            FadeWaitOut.Start();
        }

        public void Pause()
        {
            if (PlaybackState == PlaybackState.Playing)
                WaveOut.Pause();
        }

        public void Resume()
        {
            if (PlaybackState == PlaybackState.Paused)
                WaveOut.Play();
        }

        public void Stop()
        {
            if (!Fading)
            {
                WaveOut.Stop();
                WaveOut.Dispose();
                loopStream?.Dispose();
            }
        }

        public void VolumeContrl(float volume)
        {
            WaveOut.Volume = volume;
        }

        #endregion

        #region Public Properties

        public WaveOutEvent WaveOut { get; set; } = new WaveOutEvent();

        public PlaybackState PlaybackState => WaveOut.PlaybackState;

        public bool Fading { get; set; }

        #endregion

        #region Private Fields

        private LoopStream loopStream;

        private DelayFadeOutSampleProvider fader;

        #endregion
    }
}