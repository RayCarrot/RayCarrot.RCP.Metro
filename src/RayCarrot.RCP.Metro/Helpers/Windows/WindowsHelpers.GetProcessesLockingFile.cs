using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RayCarrot.RCP.Metro;

public static partial class WindowsHelpers
{
    public static Process[] GetProcessesLockingFile(string path)
    {
        string key = Guid.NewGuid().ToString();

        int result = RmStartSession(out uint handle, 0, key);
        
        if (result != 0)
            throw new Win32Exception("Could not start restart session");

        try
        {
            const int ERROR_MORE_DATA = 234;
            uint pnProcInfo = 0;
            uint lpdwRebootReasons = 0; // None
            string[] resources = { path };

            result = RmRegisterResources(handle, (uint)resources.Length, resources, 0, null, 0, null);
            if (result != 0)
                throw new Win32Exception("Could not register resource");

            result = RmGetList(handle, out uint pnProcInfoNeeded, ref pnProcInfo, null, ref lpdwRebootReasons);

            if (result != ERROR_MORE_DATA && result != 0)
                throw new Win32Exception("Could not get process list size");

            RM_PROCESS_INFO[] processInfo = new RM_PROCESS_INFO[pnProcInfoNeeded];
            pnProcInfo = pnProcInfoNeeded;

            // Get the list
            result = RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, processInfo, ref lpdwRebootReasons);

            if (result != 0)
                throw new Win32Exception("Could not get process list");

            Process[] processes = new Process[pnProcInfo];

            for (int i = 0; i < pnProcInfo; i++)
                processes[i] = Process.GetProcessById(processInfo[i].Process.dwProcessId);

            return processes;
        }
        finally
        {
            RmEndSession(handle);
        }
    }

    private const int CCH_RM_MAX_APP_NAME = 255;
    private const int CCH_RM_MAX_SVC_NAME = 63;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct RM_PROCESS_INFO
    {
        public RM_UNIQUE_PROCESS Process;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_APP_NAME + 1)]
        public string strAppName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_SVC_NAME + 1)]
        public string strServiceShortName;
        public RM_APP_TYPE ApplicationType;
        public uint AppStatus;
        public uint TSSessionId;
        [MarshalAs(UnmanagedType.Bool)]
        public bool bRestartable;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RM_UNIQUE_PROCESS
    {
        public int dwProcessId;
        public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
    }

    private enum RM_APP_TYPE
    {
        RmUnknownApp = 0,
        RmMainWindow = 1,
        RmOtherWindow = 2,
        RmService = 3,
        RmExplorer = 4,
        RmConsole = 5,
        RmCritical = 1000
    }

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int RmGetList(
        uint dwSessionHandle,
        out uint pnProcInfoNeeded,
        ref uint pnProcInfo,
        [In, Out] RM_PROCESS_INFO[]? rgAffectedApps,
        ref uint lpdwRebootReasons);

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int RmRegisterResources(
        uint pSessionHandle, 
        uint nFiles, 
        string[] rgsFilenames,
        uint nApplications,
        [In] RM_UNIQUE_PROCESS[]? rgApplications,
        uint nServices, 
        string[]? rgsServiceNames);

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int RmEndSession(uint pSessionHandle);
}