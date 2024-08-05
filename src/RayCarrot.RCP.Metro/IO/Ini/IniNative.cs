using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace RayCarrot.RCP.Metro.Ini;

public static class IniNative
{
    private const int SectionBufferSize = 0x7FFF;
    private const int ValueBufferSize = 0x200;

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    private static extern uint GetPrivateProfileSection(
        string lpAppName, 
        IntPtr lpszReturnBuffer,
        uint nSize, 
        string lpFileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern uint GetPrivateProfileString(
        string lpAppName,
        string lpKeyName,
        string lpDefault,
        StringBuilder lpReturnedString,
        uint nSize,
        string lpFileName);

    [DllImport("kernel32.dll")]
    private static extern uint GetPrivateProfileInt(
        string lpAppName,
        string lpKeyName,
        int nDefault,
        string lpFileName);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool WritePrivateProfileSection(
        string lpAppName,
        string lpString, 
        string lpFileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool WritePrivateProfileString(
        string lpAppName,
        string lpKeyName, 
        string lpString, 
        string lpFileName);

    public static string GetSection(FileSystemPath filePath, string appName)
    {
        IntPtr allocatedStringBuffer = Marshal.AllocCoTaskMem(SectionBufferSize);

        try
        {
            uint stringLength = GetPrivateProfileSection(appName, allocatedStringBuffer, SectionBufferSize, filePath);

            if (stringLength == 0)
                return String.Empty;

            return Marshal.PtrToStringAuto(allocatedStringBuffer, (int)stringLength);
        }
        finally
        {
            Marshal.FreeCoTaskMem(allocatedStringBuffer);
        }
    }

    public static string[] GetSectionValues(FileSystemPath filePath, string appName)
    {
        IntPtr allocatedStringBuffer = Marshal.AllocCoTaskMem(SectionBufferSize);

        try
        {
            uint stringLength = GetPrivateProfileSection(appName, allocatedStringBuffer, SectionBufferSize, filePath);

            if (stringLength == 0)
                return Array.Empty<string>();

            string stringValue = Marshal.PtrToStringAuto(allocatedStringBuffer, (int)stringLength);

            return stringValue.Substring(0, stringValue.Length - 1).Split('\0');
        }
        finally
        {
            Marshal.FreeCoTaskMem(allocatedStringBuffer);
        }
    }

    public static string GetString(FileSystemPath filePath, string appName, string keyName, string defaultValue)
    {
        StringBuilder sb = new(ValueBufferSize);
        GetPrivateProfileString(appName, keyName, defaultValue, sb, ValueBufferSize, filePath);
        return sb.ToString();
    }

    public static int GetInt(FileSystemPath filePath, string appName, string keyName, int defaultValue)
    {
        return (int)GetPrivateProfileInt(appName, keyName, defaultValue, filePath);
    }

    public static void WriteSection(FileSystemPath filePath, string appName, string section)
    {
        bool success = WritePrivateProfileSection(appName, section, filePath);

        if (!success)
            throw new Win32Exception();
    }

    public static void WriteString(FileSystemPath filePath, string appName, string keyName, string value)
    {
        bool success = WritePrivateProfileString(appName, keyName, value, filePath);

        if (!success)
            throw new Win32Exception();
    }
}