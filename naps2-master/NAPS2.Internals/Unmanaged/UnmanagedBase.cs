using System.Runtime.InteropServices;

namespace NAPS2.Unmanaged;

/// <summary>
/// Base class for implicitly converting structures to unmanaged objects addressed by IntPtr.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class UnmanagedBase<T> : IDisposable
{
    private bool _disposed;

    ~UnmanagedBase()
    {
        Dispose();
    }

    /// <summary>
    /// Gets the size of the unmanaged structure in bytes. If the structure is null, this is zero.
    /// </summary>
    protected int Size { get; init; }

    /// <summary>
    /// Gets a pointer to the unmanaged structure. If the provided value was null, this is IntPtr.Zero.
    /// </summary>
    protected IntPtr Pointer { get; init; }

    /// <summary>
    /// Gets a managed copy of the unmanaged structure.
    /// </summary>
    public T Value
    {
        get
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("unmanaged");
            }
            return GetValue();
        }
    }

    public void Dispose()
    {
        if (Pointer != IntPtr.Zero && !_disposed)
        {
            DestroyStructures();
            Marshal.FreeHGlobal(Pointer);
        }
        _disposed = true;
    }

    protected abstract T GetValue();

    protected abstract void DestroyStructures();

    public static implicit operator IntPtr(UnmanagedBase<T> unmanaged) => unmanaged.Pointer;
}