using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.UI;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    public class Watchdog : IDisposable
    {
        #region Constructor

        /// <summary>
        /// Creates a new instance of <see cref="Watchdog"/>
        /// </summary>
        /// <param name="raymanVersion">The Rayman version to watch for</param>
        /// <param name="dosBoxVersion">The DOSBox version to watch for</param>
        /// <param name="players">The players</param>
        public Watchdog(TPLSRaymanVersion raymanVersion, TPLSDOSBoxVersion dosBoxVersion, params SoundtrackPlayer[] players)
        {
            // Set properties
            RaymanVersion = raymanVersion;
            DOSBoxVersion = dosBoxVersion;
            Players = players;
        }

        #endregion

        #region External Methods

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        #endregion

        #region Private Properties

        /// <summary>
        /// The Rayman version to search for
        /// </summary>
        private TPLSRaymanVersion RaymanVersion { get; set; }

        /// <summary>
        /// The DOSBox version to search for
        /// </summary>
        private TPLSDOSBoxVersion DOSBoxVersion { get; }

        /// <summary>
        /// The DOSBox process
        /// </summary>
        private Process Process { get; set; }

        /// <summary>
        /// The DOSBox process handle
        /// </summary>
        private IntPtr ProcessHandle { get; set; }

        /// <summary>
        /// The players
        /// </summary>
        private SoundtrackPlayer[] Players { get; }

        #endregion

        #region Private Fields

        private const int PROCESS_WM_READ = 0x0010;
        private string Level;
        private string World;
        private byte RaymanInLevel;
        private byte MusicOnOff;
        private byte OptionsOn;
        private byte OptionsOff;
        private byte BossEvent;
        private short XAxis;
        private short YAxis;
        private float Volume = 100;
        private int bytesRead;

        #endregion

        #region Public Events

        public event EventHandler<ValueEventArgs<string>> LevelChanged;

        public event EventHandler<ValueEventArgs<string>> WorldChanged;

        public event EventHandler<ValueEventArgs<bool>> RaymanInLevelChanged;

        public event EventHandler<ValueEventArgs<bool>> MusicOnOffChanged;

        public event EventHandler<ValueEventArgs<bool>> OptionsOnChanged;

        public event EventHandler<ValueEventArgs<bool>> OptionsOffChanged;

        public event EventHandler<ValueEventArgs<bool>> BossEventChanged;

        public event EventHandler<ValueEventArgs<short>> XAxisChanged;

        public event EventHandler<ValueEventArgs<short>> YAxisChanged;

        public event EventHandler<ValueEventArgs<float>> VolumeChanged;

        #endregion

        #region Public Methods

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Process?.Dispose();
            Players.ForEach(x => x.Stop());
        }

        /// <summary>
        /// Starts watching for Rayman and runs TPLS until it closes
        /// </summary>
        /// <param name="dosBoxProcess">The DOSBox process, or null to auto detect</param>
        /// <param name="token">The cancellation token</param>
        public async Task StartWatchingRaymanAsync(Process dosBoxProcess, CancellationToken token)
        {
            RCFCore.Logger?.LogInformationSource("TPLS: TPLS has started searching for Rayman");

            int eAX = 0;
            byte[] baseBuffer = new byte[4];

            // Set a stop watch
            var sw = Stopwatch.StartNew();

            // Loop to find the DOSBox process with Rayman, running for 10 seconds
            while (eAX == 0)
            {
                // Stop searching after 20 seconds
                if (sw.Elapsed.Seconds > 20)
                {
                    RCFCore.Logger?.LogInformationSource("TPLS: Search has timed out");
                    return;
                }

                try
                {
                    // Check if the service has been canceled
                    token.ThrowIfCancellationRequested();

                    // Get the process
                    Process = dosBoxProcess;

                    // If the process if null, attempt to get it by name
                    if (Process == null)
                    {
                        var p = Process.GetProcessesByName("DOSBox");

                        // If any processes were found, save the first one
                        if (p.Any())
                            Process = p.First();
                    }

                    // If the process is still null, check again after a delay
                    if (Process == null)
                    {
                        await Task.Delay(50, token);
                        continue;
                    }

                    // Get the process handle using P/Invoke
                    ProcessHandle = OpenProcess(PROCESS_WM_READ, false, Process.Id);
                    
                    if (!ReadProcessMemory((int)ProcessHandle, DOSBoxVersion == TPLSDOSBoxVersion.DOSBox_0_74 ? 0x74B6B0 : DOSBoxVersion == TPLSDOSBoxVersion.DOSBox_SVN_Daum ? 0x8B5B84 : throw new IndexOutOfRangeException(), baseBuffer, 4, ref bytesRead))
                        throw new Win32Exception();

                    // Convert the buffer to an integer
                    eAX = BitConverter.ToInt32(baseBuffer, 0);

                    // Verify memory
                    if (eAX == 0)
                    {
                        await Task.Delay(50, token);
                        continue;
                    }

                    if (!ReadProcessMemory((int)ProcessHandle, eAX, baseBuffer, 4, ref bytesRead))
                        throw new Win32Exception();

                    if (baseBuffer[0] != 0x60 && baseBuffer[1] != 0x10 && baseBuffer[2] != 0x00 && baseBuffer[3] != 0xF0)
                        eAX = 0;

                    // Attempt to detect Rayman version
                    if (RaymanVersion == TPLSRaymanVersion.Auto)
                    {
                        // TODO: Look for remaining versions

                        if (!ReadProcessMemory((int)ProcessHandle, eAX + 0x16D7BC, baseBuffer, 4, ref bytesRead))
                            throw new Win32Exception();

                        if (BitConverter.ToInt32(baseBuffer, 0) == 320)
                            RaymanVersion = TPLSRaymanVersion.Ray_1_12_0;
                        else
                        {
                            if (!ReadProcessMemory((int)ProcessHandle, eAX + 0x16E87C, baseBuffer, 4, ref bytesRead))
                                throw new Win32Exception();

                            if (BitConverter.ToInt32(baseBuffer, 0) == 320)
                                RaymanVersion = TPLSRaymanVersion.Ray_1_20;
                            else
                            {
                                if (!ReadProcessMemory((int)ProcessHandle, eAX + 0x16E7EC, baseBuffer, 4, ref bytesRead))
                                    throw new Win32Exception();

                                if (BitConverter.ToInt32(baseBuffer, 0) == 320)
                                    RaymanVersion = TPLSRaymanVersion.Ray_1_21;
                                else
                                    eAX = 0;
                            }
                        }
                    }
                }
                catch(OperationCanceledException ex)
                {
                    ex.HandleExpected("Finding DOSBox process and Rayman game");

                    // Stop the stop watch
                    sw.Stop();

                    return;
                }
                catch (Exception ex)
                {
                    ex.HandleUnexpected("Finding DOSBox process and Rayman game");

                    // Reset
                    eAX = 0;

                    // Wait 200 milliseconds before trying again
                    await Task.Delay(200, token);
                }
            }

            // Stop the stop watch
            sw.Stop();

            RCFCore.Logger?.LogInformationSource($"TPLS: Rayman version {RaymanVersion} detected at {eAX:X} using DOSBox version {DOSBoxVersion}");

            // Begin refreshing for the game
            // Add the world base offset as it is always loaded in first
            await RefreshAsync(eAX + RaymanVersion.GetWorldBase(), token);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Refreshes Rayman values until the game closes
        /// </summary>
        /// <param name="realAddress">The address</param>
        /// <param name="token">The cancellation token</param>
        private async Task RefreshAsync(int realAddress, CancellationToken token)
        {
            try
            {
                // Create the buffer to read the memory into
                byte[] buffer = new byte[RaymanVersion.GetBufferSize()];

                // Update values until DOSBox closes
                while (!Process.HasExited)
                {
                    token.ThrowIfCancellationRequested();

                    bytesRead = 0;

                    // Read the memory
                    ReadProcessMemory((int)ProcessHandle, realAddress, buffer, buffer.Length, ref bytesRead);

                    // Get the values
                    string world = Encoding.ASCII.GetString(buffer, 0, 8).Split('.').First();
                    string level = Encoding.ASCII.GetString(buffer, RaymanVersion.GetLevel(), 8).Split('.').First();

                    byte raymanInLevel = buffer[RaymanVersion.GetInLevel()];
                    byte musicOnOff = buffer[RaymanVersion.GetMusicOnOff()];
                    byte optionsOn = buffer[RaymanVersion.GetOptionsOn()];
                    byte optionsOff = buffer[RaymanVersion.GetOptionsOff()];
                    byte bossEvent = buffer[RaymanVersion.GetBossEvent()];

                    byte[] xAxisByte =
                    {
                        buffer[RaymanVersion.GetXAxis()],
                        buffer[RaymanVersion.GetXAxis() + 1]
                    };
                    byte[] yAxisByte =
                    {
                        buffer[RaymanVersion.GetYAxis()],
                        buffer[RaymanVersion.GetYAxis() + 1]
                    };

                    short xAxis = BitConverter.ToInt16(xAxisByte, 0);
                    short yAxis = BitConverter.ToInt16(yAxisByte, 0);

                    if (XAxis != xAxis)
                    {
                        XAxis = xAxis;
                        XAxisChanged?.Invoke(this, new ValueEventArgs<short>(XAxis));
                    }

                    if (YAxis != yAxis)
                    {
                        YAxis = yAxis;
                        YAxisChanged?.Invoke(this, new ValueEventArgs<short>(YAxis));
                    }

                    if (Level != level)
                    {
                        Level = level;
                        LevelChanged?.Invoke(this, new ValueEventArgs<string>(Level));

                        RCFCore.Logger?.LogInformationSource($"TPLS: Level has changed to {Level}");
                    }

                    if (World != world)
                    {
                        World = world;
                        WorldChanged?.Invoke(this, new ValueEventArgs<string>(World));

                        RCFCore.Logger?.LogInformationSource($"TPLS: World has changed to {World}");
                    }

                    if (OptionsOn != optionsOn)
                    {
                        OptionsOn = optionsOn;
                        OptionsOnChanged?.Invoke(this,
                            OptionsOn == 1 ? new ValueEventArgs<bool>(true) : new ValueEventArgs<bool>(false));

                        RCFCore.Logger?.LogInformationSource($"TPLS: OptionsOn has changed to {OptionsOn}");
                    }

                    if (OptionsOff != optionsOff)
                    {
                        OptionsOff = optionsOff;
                        OptionsOffChanged?.Invoke(this, new ValueEventArgs<bool>(OptionsOff == 1));

                        RCFCore.Logger?.LogInformationSource($"TPLS: OptionsOff has changed to {OptionsOff}");
                    }

                    if (MusicOnOff != musicOnOff)
                    {
                        MusicOnOff = musicOnOff;
                        MusicOnOffChanged?.Invoke(this, MusicOnOff == 0 ? new ValueEventArgs<bool>(false) : new ValueEventArgs<bool>(true));

                        RCFCore.Logger?.LogInformationSource($"TPLS: MusicOnOff has changed to {MusicOnOff}");
                    }

                    if (RaymanInLevel != raymanInLevel)
                    {
                        RaymanInLevel = raymanInLevel;
                        RaymanInLevelChanged?.Invoke(this, new ValueEventArgs<bool>(RaymanInLevel == 1));

                        RCFCore.Logger?.LogInformationSource($"TPLS: RaymanInLevel has changed to {RaymanInLevel}");
                    }

                    if (BossEvent != bossEvent)
                    {
                        BossEvent = bossEvent;
                        BossEventChanged?.Invoke(this, new ValueEventArgs<bool>(BossEvent == 1));

                        RCFCore.Logger?.LogInformationSource($"TPLS: BossEvent has changed to {BossEvent}");
                    }

                    if (Volume != VolumeMixer.GetApplicationVolume(Process.Id))
                    {
                        Volume = VolumeMixer.GetApplicationVolume(Process.Id) ?? Volume;
                        VolumeChanged?.Invoke(this, new ValueEventArgs<float>(Volume / 100f));
                    }

                    await Task.Delay(10, token);
                }

                RCFCore.Logger?.LogInformationSource("TPLS: TPLS has stopped due to the DOSBox process having exited");
            }
            catch (OperationCanceledException ex)
            {
                ex.HandleExpected("TPLS");
            }
            catch (Exception ex)
            {
                ex.HandleError("TPLS");

                if (Process.HasExited || !await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.TPLS_Error, ex.Message), Resources.TPLS_ErrorHeader, MessageType.Error, true))
                    return;

                await StartWatchingRaymanAsync(Process, token);
            }
        }

        #endregion
    }
}