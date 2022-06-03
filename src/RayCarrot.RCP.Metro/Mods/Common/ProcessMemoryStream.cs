using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace RayCarrot.RCP.Metro;

public class ProcessMemoryStream : Stream
{
    #region Constructor

    public ProcessMemoryStream(Process process, Mode accessMode)
    {
        // Set properties
        _process = process;
        AccessMode = accessMode;

        int accessLevel = accessMode switch
        {
            Mode.Read => PROCESS_WM_READ,
            Mode.Write => PROCESS_VM_WRITE,
            Mode.AllAccess => PROCESS_ALL_ACCESS,
            _ => PROCESS_WM_READ
        };

        // Open the process and get the handle
        _processHandle = OpenProcess(accessLevel, false, process.Id);

        if (_processHandle == IntPtr.Zero)
            throw new Win32Exception("Failed to open process");
    }

    #endregion

    #region Private Fields

    private long _currentAddress;
    private IntPtr _processHandle;
    private Process? _process;

    #endregion

    #region Public Properties

    public Process Process => _process ?? throw new ObjectDisposedException(nameof(ProcessMemoryStream));

    public Mode AccessMode { get; set; }

    public long BaseStreamOffset { get; set; }

    public long BaseAddress => Process.MainModule!.BaseAddress.ToInt64();
    public int MemorySize => Process.MainModule!.ModuleMemorySize;

    public override bool CanRead => AccessMode is Mode.Read or Mode.AllAccess && _processHandle != IntPtr.Zero;
    public override bool CanSeek => _processHandle != IntPtr.Zero;
    public override bool CanWrite => AccessMode is Mode.Write or Mode.AllAccess && _processHandle != IntPtr.Zero;

    public override long Length
    {
        get
        {
            if (!CanSeek)
                throw new NotSupportedException();

            return MemorySize - BaseStreamOffset;
        }
    }
    public override long Position
    {
        get
        {
            if (!CanSeek)
                throw new NotSupportedException();

            return _currentAddress - BaseStreamOffset;
        }
        set
        {
            if (!CanSeek)
                throw new NotSupportedException();

            _currentAddress = value + BaseStreamOffset;
        }
    }

    #endregion

    #region P/Invoke

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool ReadProcessMemory(IntPtr hProcess, long lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteProcessMemory(IntPtr hProcess, long lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);

    private const int PROCESS_ALL_ACCESS = 0x1F0FFF;
    private const int PROCESS_WM_READ = 0x0010;
    private const int PROCESS_VM_WRITE = 0x0020;

    #endregion

    #region Public Methods

    public override void Flush() { }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (!CanRead)
            throw new NotSupportedException();

        byte[] readBuffer = new byte[count];

        bool success = ReadProcessMemory(_processHandle, _currentAddress, readBuffer, count, out int numBytesRead);

        if (!success)
            throw new Win32Exception();

        if (numBytesRead == 0)
            return 0;

        Seek(numBytesRead, SeekOrigin.Current);
        Array.Copy(readBuffer, 0, buffer, offset, numBytesRead);
        return numBytesRead;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        if (!CanSeek)
            throw new NotSupportedException();

        switch (origin)
        {
            case SeekOrigin.Begin:
                _currentAddress = offset + BaseStreamOffset;
                break;

            case SeekOrigin.Current:
                _currentAddress += offset;
                break;

            case SeekOrigin.End:
                _currentAddress = BaseAddress + MemorySize - offset;
                break;
        }

        return _currentAddress - BaseStreamOffset;
    }

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (!CanWrite)
            throw new NotSupportedException();

        byte[] writeBuffer = new byte[count];

        Array.Copy(buffer, offset, writeBuffer, 0, count);

        bool success = WriteProcessMemory(_processHandle, _currentAddress, writeBuffer, count, out int numBytesWritten);

        if (!success)
            throw new Win32Exception();

        if (numBytesWritten != 0)
            Seek(numBytesWritten, SeekOrigin.Current);
    }

    ~ProcessMemoryStream() => Dispose(false);

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (_processHandle != IntPtr.Zero)
        {
            CloseHandle(_processHandle);
            _processHandle = IntPtr.Zero;
        }

        if (_process != null)
        {
            _process.Dispose();
            _process = null;
        }
    }

    #endregion

    #region Enums

    public enum Mode
    {
        Read,
        Write,
        AllAccess,
    }

    #endregion
}