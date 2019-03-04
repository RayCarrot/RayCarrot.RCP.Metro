using RayCarrot.CarrotFramework;
using System;
using System.Diagnostics;
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
        public Watchdog(TPLSRaymanVersion raymanVersion, TPLSDOSBoxVersion dosBoxVersion)
        {
            // Set properties
            RaymanVersion = raymanVersion;
            DOSBoxVersion = dosBoxVersion;
        }

        #endregion

        #region External Methods

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(int hProcess,
          int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

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
        }

        /// <summary>
        /// Starts watching for Rayman and runs TPLS until it closes
        /// </summary>
        /// <param name="dosBoxProcess">The DOSBox process, or null to auto detect</param>
        public async Task StartWatchingRaymanAsync(Process dosBoxProcess = null)
        {
            RCF.Logger.LogInformationSource("TPLS: TPLS has started searching for Rayman");

            int eAX = 0;
            byte[] baseBuffer = new byte[4];

            // Set a stop watch
            var sw = Stopwatch.StartNew();

            // Loop to find the DOSBox process with Rayman, running for 10 seconds
            while (eAX == 0)
            {
                // Stop searching after 10 seconds
                if (sw.Elapsed.Seconds > 10)
                {
                    RCF.Logger.LogInformationSource("TPLS: Search has timed out");
                    return;
                }

                try
                {
                    // Get the process
                    Process = dosBoxProcess ?? Process.GetProcessesByName("DOSBox")[0];

                    // Get the handle using P/Invoke
                    ProcessHandle = OpenProcess(PROCESS_WM_READ, false, Process.Id);

                    // Read memory to verify it is DOSBox
                    ReadProcessMemory((int)ProcessHandle, 
                        DOSBoxVersion == TPLSDOSBoxVersion.DOSBox_0_74 ? 0x74B6B0 :
                        DOSBoxVersion == TPLSDOSBoxVersion.DOSBox_SVN_Daum ? 0x8B5B84 : throw new IndexOutOfRangeException()
                        , baseBuffer, 4, ref bytesRead);
                    eAX = BitConverter.ToInt32(baseBuffer, 0);

                    // Verify memory
                    if (eAX == 0)
                    {
                        await Task.Delay(50);
                        continue;
                    }

                    ReadProcessMemory((int)ProcessHandle, eAX, baseBuffer, 4, ref bytesRead);
                    if (baseBuffer[0] != 0x60 && baseBuffer[1] != 0x10 && baseBuffer[2] != 0x00 && baseBuffer[3] != 0xF0)
                        eAX = 0;

                    // Attempt to detect Rayman version
                    if (RaymanVersion == TPLSRaymanVersion.Auto)
                    {
                        ReadProcessMemory((int)ProcessHandle, eAX + 0x16D7BC, baseBuffer, 4, ref bytesRead);
                        if (BitConverter.ToInt32(baseBuffer, 0) == 320)
                            RaymanVersion = TPLSRaymanVersion.Ray_1_12;
                        else
                        {
                            ReadProcessMemory((int)ProcessHandle, eAX + 0x16E87C, baseBuffer, 4, ref bytesRead);
                            if (BitConverter.ToInt32(baseBuffer, 0) == 320)
                                RaymanVersion = TPLSRaymanVersion.Ray_1_20;
                            else
                            {
                                ReadProcessMemory((int)ProcessHandle, eAX + 0x16E7EC, baseBuffer, 4, ref bytesRead);
                                if (BitConverter.ToInt32(baseBuffer, 0) == 320)
                                    RaymanVersion = TPLSRaymanVersion.Ray_1_21;
                                else
                                    eAX = 0;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.HandleUnexpected("Finding DOSBox process and Rayman game");

                    // Wait 50 milliseconds before trying again
                    await Task.Delay(50);
                }
            }

            // Stop the stop watch
            sw.Stop();

            RCF.Logger.LogInformationSource($"TPLS: Rayman version {RaymanVersion} detected at {eAX.ToString("X")} using DOSBox version {DOSBoxVersion}");

            // Begin refreshing for the game
            await RefreshAsync(eAX + RaymanVersion.GetWorldBase());
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Refreshes Rayman values until the game closes
        /// </summary>
        /// <param name="RealAddress">The address</param>
        private async Task RefreshAsync(int RealAddress)
        {
            try
            {
                // Update values until DOSBox closes
                while (!Process.HasExited)
                {
                    byte[] buffer = new byte[0x17526];
                    int bytesRead = 0;

                    // Read the memory
                    ReadProcessMemory((int)ProcessHandle, RealAddress, buffer, buffer.Length, ref bytesRead);

                    // Get the values
                    string world = Encoding.ASCII.GetString(buffer, 0x00000, 8).Split('.')[0];
                    string level = Encoding.ASCII.GetString(buffer, RaymanVersion.GetLevel(), 8).Split('.')[0];

                    byte raymanInLevel = buffer[RaymanVersion.GetInLevel()];

                    byte musicOnOff = buffer[RaymanVersion.GetMusicOnOff()];

                    byte optionsOn = buffer[RaymanVersion.GetOptionsOn()];

                    byte optionsOff = buffer[RaymanVersion.GetOptionsOff()];

                    byte bossEvent = buffer[RaymanVersion.GetBossEvent()];

                    byte[] xAxisByte = { buffer[RaymanVersion.GetXAxis()], buffer[RaymanVersion.GetXAxis() + 1] };
                    byte[] yAxisByte = { buffer[RaymanVersion.GetYAxis()], buffer[RaymanVersion.GetYAxis() + 1] };

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

                        RCF.Logger.LogInformationSource($"TPLS: Level has changed to {Level}");
                    }

                    if (World != world)
                    {
                        World = world;
                        WorldChanged?.Invoke(this, new ValueEventArgs<string>(World));

                        RCF.Logger.LogInformationSource($"TPLS: World has changed to {World}");
                    }

                    if (OptionsOn != optionsOn)
                    {
                        OptionsOn = optionsOn;
                        OptionsOnChanged?.Invoke(this,
                            OptionsOn == 1 ? new ValueEventArgs<bool>(true) : new ValueEventArgs<bool>(false));

                        RCF.Logger.LogInformationSource($"TPLS: OptionsOn has changed to {OptionsOn}");
                    }

                    if (OptionsOff != optionsOff)
                    {
                        OptionsOff = optionsOff;
                        OptionsOffChanged?.Invoke(this,
                            OptionsOff == 1 ? new ValueEventArgs<bool>(true) : new ValueEventArgs<bool>(false));

                        RCF.Logger.LogInformationSource($"TPLS: OptionsOff has changed to {OptionsOff}");
                    }

                    if (MusicOnOff != musicOnOff)
                    {
                        MusicOnOff = musicOnOff;
                        MusicOnOffChanged?.Invoke(this,
                            MusicOnOff == 0 ? new ValueEventArgs<bool>(false) : new ValueEventArgs<bool>(true));

                        RCF.Logger.LogInformationSource($"TPLS: MusicOnOff has changed to {MusicOnOff}");
                    }

                    if (RaymanInLevel != raymanInLevel)
                    {
                        RaymanInLevel = raymanInLevel;
                        RaymanInLevelChanged?.Invoke(this,
                            RaymanInLevel == 1 ? new ValueEventArgs<bool>(true) : new ValueEventArgs<bool>(false));

                        RCF.Logger.LogInformationSource($"TPLS: RaymanInLevel has changed to {RaymanInLevel}");
                    }

                    if (BossEvent != bossEvent)
                    {
                        BossEvent = bossEvent;
                        BossEventChanged?.Invoke(this,
                            BossEvent == 1 ? new ValueEventArgs<bool>(true) : new ValueEventArgs<bool>(false));

                        RCF.Logger.LogInformationSource($"TPLS: BossEvent has changed to {BossEvent}");
                    }

                    if (Volume != VolumeMixer.GetApplicationVolume(Process.Id))
                    {
                        Volume = VolumeMixer.GetApplicationVolume(Process.Id) ?? Volume;
                        VolumeChanged?.Invoke(this, new ValueEventArgs<float>(Volume / 100f));
                    }

                    await Task.Delay(10);
                }

                RCF.Logger.LogInformationSource("TPLS: TPLS has stopped due to the DOSBox process having exited");
            }
            catch (Exception ex)
            {
                ex.HandleError("TPLS");

                if (!await RCF.MessageUI.DisplayMessageAsync($"The PlayStation soundtrack utility crashed with the error message of: {ex.Message}{Environment.NewLine}Restart?", "TPLS Error", MessageType.Error, true))
                    return;

                await StartWatchingRaymanAsync();
            }
        }

        #endregion
    }
}