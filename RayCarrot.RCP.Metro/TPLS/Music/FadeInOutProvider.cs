using System.Collections.Generic;
using NAudio.Wave;

namespace RayCarrot.RCP.Metro
{
    public class DelayFadeOutSampleProvider : ISampleProvider
    {
        #region Public Methods

        /// <summary>
        /// Creates a new FadeInOutSampleProvider
        /// </summary>
        /// <param name="source">The source stream with the audio to be faded in or out</param>
        /// <param name="initiallySilent">If true, we start faded out</param>
        public DelayFadeOutSampleProvider(ISampleProvider source, bool initiallySilent = false)
        {
            this.source = source;
            fadeState = initiallySilent ? FadeState.Silence : FadeState.FullVolume;
        }

        /// <summary>
        /// Requests that a fade-in begins (will start on the next call to Read)
        /// </summary>
        /// <param name="fadeDurationInMilliseconds">Duration of fade in milliseconds</param>
        public void BeginFadeIn(double fadeDurationInMilliseconds)
        {
            lock (lockObject)
            {
                fadeSamplePosition = 0;
                fadeSampleCount = (int)((fadeDurationInMilliseconds * source.WaveFormat.SampleRate) / 1000);
                fadeState = FadeState.FadingIn;
            }
        }

        /// <summary>
        /// Requests that a fade-out begins (will start on the next call to Read)
        /// </summary>
        /// <param name="fadeDurationInMilliseconds">Duration of fade in milliseconds</param>
        public void BeginFadeOut(double fadeAfterMilliseconds, double fadeDurationInMilliseconds)
        {
            lock (lockObject)
            {
                fadeSamplePosition = 0;
                fadeSampleCount = (int)((fadeDurationInMilliseconds * source.WaveFormat.SampleRate) / 1000);
                fadeOutDelaySamples = (int)((fadeAfterMilliseconds * source.WaveFormat.SampleRate) / 1000);
                fadeOutDelayPosition = 0;
            }
        }

        /// <summary>
        /// Reads samples from this sample provider
        /// </summary>
        /// <param name="buffer">Buffer to read into</param>
        /// <param name="offset">Offset within buffer to write to</param>
        /// <param name="count">Number of samples desired</param>
        /// <returns>Number of samples read</returns>
        public int Read(float[] buffer, int offset, int count)
        {
            int sourceSamplesRead = source.Read(buffer, offset, count);

            lock (lockObject)
            {
                if (fadeOutDelaySamples > 0)
                {
                    fadeOutDelayPosition += sourceSamplesRead / WaveFormat.Channels;
                    if (fadeOutDelayPosition >= fadeOutDelaySamples)
                    {
                        fadeOutDelaySamples = 0;
                        fadeState = FadeState.FadingOut;
                    }
                }
                if (fadeState == FadeState.FadingIn)
                {
                    FadeIn(buffer, offset, sourceSamplesRead);
                }
                else if (fadeState == FadeState.FadingOut)
                {
                    FadeOut(buffer, offset, sourceSamplesRead);
                }
                else if (fadeState == FadeState.Silence)
                {
                    ClearBuffer(buffer, offset, count);
                }
            }
            return sourceSamplesRead;
        }

        #endregion

        #region Private Fields

        private readonly object lockObject = new object();

        private readonly ISampleProvider source;

        private int fadeSamplePosition;

        private int fadeSampleCount;

        private int fadeOutDelaySamples;

        private int fadeOutDelayPosition;

        private FadeState fadeState;

        #endregion

        #region Private Methods

        private static void ClearBuffer(IList<float> buffer, int offset, int count)
        {
            for (int n = 0; n < count; n++)
                buffer[n + offset] = 0;
        }

        private void FadeOut(IList<float> buffer, int offset, int sourceSamplesRead)
        {
            int sample = 0;
            while (sample < sourceSamplesRead)
            {
                float multiplier = 1.0f - (fadeSamplePosition / (float)fadeSampleCount);

                for (int ch = 0; ch < source.WaveFormat.Channels; ch++)
                    buffer[offset + sample++] *= multiplier;

                fadeSamplePosition++;

                if (fadeSamplePosition <= fadeSampleCount)
                    continue;

                fadeState = FadeState.Silence;
                // clear out the end
                ClearBuffer(buffer, sample + offset, sourceSamplesRead - sample);
                break;
            }
        }

        private void FadeIn(IList<float> buffer, int offset, int sourceSamplesRead)
        {
            int sample = 0;
            while (sample < sourceSamplesRead)
            {
                float multiplier = (fadeSamplePosition / (float)fadeSampleCount);
                for (int ch = 0; ch < source.WaveFormat.Channels; ch++)
                    buffer[offset + sample++] *= multiplier;

                fadeSamplePosition++;

                if (fadeSamplePosition <= fadeSampleCount)
                    continue;

                fadeState = FadeState.FullVolume;
                // no need to multiply any more
                break;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// WaveFormat of this SampleProvider
        /// </summary>
        public WaveFormat WaveFormat => source.WaveFormat;

        #endregion

        /// <summary>
        /// The available fade states
        /// </summary>
        private enum FadeState
        {
            Silence,
            FadingIn,
            FullVolume,
            FadingOut,
        }

    }
}