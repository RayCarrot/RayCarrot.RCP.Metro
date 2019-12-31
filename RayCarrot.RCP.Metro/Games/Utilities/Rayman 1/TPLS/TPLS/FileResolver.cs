using System.IO;
using RayCarrot.IO;
// ReSharper disable InvalidXmlDocComment

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

        /// <summary>
        /// Reads a block of bytes from the stream and writes the data in a given buffer.
        /// </summary>
        /// <param name="array">When this method returns, contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1<paramref name=")" /> replaced by the bytes read from the current source. </param>
        /// <param name="offset">The byte offset in <paramref name="array" /> at which the read bytes will be placed. </param>
        /// <param name="count">The maximum number of bytes to read. </param>
        /// <returns>The total number of bytes read into the buffer. This might be less than the number of bytes requested if that number of bytes are not currently available, or zero if the end of the stream is reached.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array" /> is <see langword="null" />. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="offset" /> or <paramref name="count" /> is negative. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="offset" /> and <paramref name="count" /> describe an invalid range in <paramref name="array" />. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override int Read(byte[] array, int streamOffset, int count)
        {
            return base.Position >= maxPos ? -1 : base.Read(array, streamOffset, count);
        }

        /// <summary>
        /// Reads a byte from the file and advances the read position one byte.
        /// </summary>
        /// <returns>The byte, cast to an <see cref="T:System.Int32" />, or -1 if the end of the stream has been reached.</returns>
        /// <exception cref="T:System.NotSupportedException">The current stream does not support reading. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The current stream is closed. </exception>
        public override int ReadByte()
        {
            return base.Position >= maxPos ? -1 : base.ReadByte();
        }

        #endregion
    }
}