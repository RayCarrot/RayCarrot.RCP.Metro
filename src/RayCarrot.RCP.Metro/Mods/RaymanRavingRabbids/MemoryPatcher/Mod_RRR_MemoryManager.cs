using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace RayCarrot.RCP.Metro;

public static class Mod_RRR_MemoryManager
{
    #region Constants

    public const int PROCESS_WM_READ = 0x0010;
    public const int PROCESS_VM_WRITE = 0x0020;
    public const int PROCESS_VM_OPERATION = 0x0008;
    public const int PROCESS_ALL_ACCESS = 0x1F0FFF;

    #endregion

    #region External Functions

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

    #endregion

    #region Read

    public static byte[] ReadProcessMemoryBytes(int processHandle, int address, int count)
    {
        int bytesRead = 0;
        byte[] buffer = new byte[count];
        var result = ReadProcessMemory(processHandle, address, buffer, count, ref bytesRead);

        if (!result)
            throw new Win32Exception();

        if (bytesRead != count)
            throw new Exception($"Only {bytesRead}/{count} bytes were read from 0x{address:X8}");

        return buffer;
    }

    public static byte ReadProcessMemoryByte(int processHandle, int address)
    {
        return ReadProcessMemoryBytes(processHandle, address, 1)[0];
    }

    public static short ReadProcessMemoryInt16(int processHandle, int address)
    {
        return BitConverter.ToInt16(ReadProcessMemoryBytes(processHandle, address, 2), 0);
    }

    public static uint ReadProcessMemoryUInt32(int processHandle, int address)
    {
        return BitConverter.ToUInt32(ReadProcessMemoryBytes(processHandle, address, 4), 0);
    }

    public static int ReadProcessMemoryInt32(int processHandle, int address)
    {
        return BitConverter.ToInt32(ReadProcessMemoryBytes(processHandle, address, 4), 0);
    }

    public static float ReadProcessMemoryFloat(int processHandle, int address)
    {
        return BitConverter.ToSingle(ReadProcessMemoryBytes(processHandle, address, 4), 0);
    }

    #endregion

    #region Write

    public static int WriteProcessMemoryBytes(int processHandle, int address, byte[] buffer, bool throwOnIncompleteWrite = true)
    {
        int bytesWritten = 0;
        var result = WriteProcessMemory(processHandle, address, buffer, buffer.Length, ref bytesWritten);

        if (!result)
            throw new Win32Exception();

        if (throwOnIncompleteWrite && bytesWritten != buffer.Length)
            throw new Exception($"Only {bytesWritten}/{buffer.Length} bytes were written to 0x{address:X8}");

        return bytesWritten;
    }

    public static int WriteProcessMemoryByte(int processHandle, int address, byte value, bool throwOnIncompleteWrite = true)
    {
        return WriteProcessMemoryBytes(processHandle, address, new byte[]
        {
            value
        }, throwOnIncompleteWrite);
    }

    public static int WriteProcessMemoryInt16(int processHandle, int address, short value, bool throwOnIncompleteWrite = true)
    {
        return WriteProcessMemoryBytes(processHandle, address, BitConverter.GetBytes(value), throwOnIncompleteWrite);
    }

    public static int WriteProcessMemoryInt32(int processHandle, int address, int value, bool throwOnIncompleteWrite = true)
    {
        return WriteProcessMemoryBytes(processHandle, address, BitConverter.GetBytes(value), throwOnIncompleteWrite);
    }

    public static int WriteProcessMemoryFloat(int processHandle, int address, float value, bool throwOnIncompleteWrite = true)
    {
        return WriteProcessMemoryBytes(processHandle, address, BitConverter.GetBytes(value), throwOnIncompleteWrite);
    }

    #endregion
}