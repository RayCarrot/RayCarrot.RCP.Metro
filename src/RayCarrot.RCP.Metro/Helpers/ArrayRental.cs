using System.Buffers;

namespace RayCarrot.RCP.Metro;

// TODO: Find more usages for this in the app? Might reduce the number of allocations needed.
public class ArrayRental<T> : IDisposable
{
    public ArrayRental(int minimumLength)
    {
        Array = ArrayPool<T>.Shared.Rent(minimumLength);
    }

    public T[] Array { get; }

    public void Dispose()
    {
        ArrayPool<T>.Shared.Return(Array);
    }
}