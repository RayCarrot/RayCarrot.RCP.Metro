using System.IO;

namespace RayCarrot.RCP.Metro;

public static class StreamExtensions
{
    // TODO: Use this in more places, such as Archive Explorer?
    public static async Task CopyToExAsync(
        this Stream source, 
        Stream destination,
        Action<Progress>? progressCallback = null, 
        CancellationToken cancellationToken = default,
        int bufferSize = 0x80000)
    {
        long? length = null;

        if (progressCallback != null)
        {
            try
            {
                length = source.Length;
            }
            catch
            {
                // Ignore
            }
        }

        Progress progress = new(0, length ?? 1);

        using ArrayRental<byte> buffer = new(bufferSize);

        int count;
        while ((count = await source.ReadAsync(buffer.Array, 0, bufferSize, cancellationToken).ConfigureAwait(false)) != 0)
        {
            await destination.WriteAsync(buffer.Array, 0, count, cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            if (length != null)
            {
                progress += count;
                progressCallback!(progress);
            }
        }

        progressCallback?.Invoke(progress.Completed());
    }
}