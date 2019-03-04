using System.IO;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    public class FileResolver : FileStream
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="path">The file path</param>
        /// <param name="offset">The offset</param>
        /// <param name="length">The length</param>
        public FileResolver(FileSystemPath path, long offset, long length) : base(path, FileMode.Open, FileAccess.Read)
        {
            base.Position = offset;
            Length = length;
            maxPos = offset + length;
            this.offset = offset;
        }

        #endregion

        #region Private Fields

        private readonly long maxPos;

        private readonly long offset;

        #endregion

        #region Public Override Properties

        public override long Position
        {
            get => base.Position + offset;
            set => base.Position = offset + value;
        }

        public override long Length { get; }

        #endregion

        #region Public Override Methods

        public override int Read(byte[] array, int offset, int count)
        {
            return base.Position >= maxPos ? -1 : base.Read(array, offset, count);
        }

        public override int ReadByte()
        {
            return base.Position >= maxPos ? -1 : base.ReadByte();
        }

        #endregion
    }
}