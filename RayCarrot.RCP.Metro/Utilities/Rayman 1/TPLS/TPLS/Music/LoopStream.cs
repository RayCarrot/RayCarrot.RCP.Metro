using NAudio.Wave;

namespace RayCarrot.RCP.Metro
{
    public class LoopStream : WaveStream
    {
        #region Constructor

        public LoopStream(WaveStream sourceStream1, WaveStream sourceStream2)
        {
            SourceStream1 = sourceStream1;
            SourceStream2 = sourceStream2;
            EnableLooping = true;
        }

        #endregion

        #region Private Properties

        private WaveStream SourceStream1 { get; }

        private WaveStream SourceStream2 { get; }

        private bool InLoop { get; set; }

        #endregion

        #region Public Properties

        public bool EnableLooping { get; set; }

        #endregion

        #region Public Override Properties

        public override int BlockAlign => InLoop ? SourceStream2.BlockAlign : SourceStream1.BlockAlign;

        public override WaveFormat WaveFormat => InLoop ? SourceStream2.WaveFormat : SourceStream1.WaveFormat;

        public override long Length => InLoop ? SourceStream2.Length : SourceStream1.Length;

        public override long Position
        {
            get => InLoop ? SourceStream2.Position : SourceStream1.Position;
            set
            {
                if (InLoop)
                    SourceStream2.Position = value;
                else
                    SourceStream1.Position = value;
            }
        }

        #endregion

        #region Public Override Methods

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;

            if (InLoop)
                return ReadLoop(buffer, offset, count, totalBytesRead);

            while (totalBytesRead < count)
            {
                int bytesRead = SourceStream1.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    if (SourceStream1.Position == 0 || !EnableLooping)
                        throw new SoundtrackUnplaybaleException("Starter");

                    InLoop = true;
                    return ReadLoop(buffer, offset, count, totalBytesRead);
                }
                totalBytesRead += bytesRead;
            }
            return totalBytesRead;
        }

        #endregion

        #region Private Methods

        private int ReadLoop(byte[] buffer, int offset, int count, int totalBytesRead)
        {
            while (totalBytesRead < count)
            {
                int bytesRead = SourceStream2.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    if (SourceStream2.Position == 0 || !EnableLooping)
                        throw new SoundtrackUnplaybaleException("Looper");
                    SourceStream2.Position = 0;
                }
                totalBytesRead += bytesRead;
            }
            return totalBytesRead;
        }

        #endregion
    }
}