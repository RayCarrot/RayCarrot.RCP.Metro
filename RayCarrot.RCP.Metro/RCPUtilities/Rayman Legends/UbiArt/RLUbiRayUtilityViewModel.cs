using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Legends UbiRay utility
    /// </summary>
    public class RLUbiRayUtilityViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public RLUbiRayUtilityViewModel()
        {
            ApplyUbiRayCommand = new AsyncRelayCommand(ApplyUbiRayAsync);

            try
            {
                AvailableSaveFiles = Directory.GetDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Rayman Legends")).
                    Select(x => new FileSystemPath(x)).Where(x => (x + "RaymanSave_0").FileExists).ToArray();
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting RL save files");
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The available save file paths
        /// </summary>
        public FileSystemPath[] AvailableSaveFiles { get; }

        /// <summary>
        /// The selected save file path
        /// </summary>
        public FileSystemPath SelectedPath { get; set; }

        #endregion

        #region Commands

        public ICommand ApplyUbiRayCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Applies the UbiRay modification
        /// </summary>
        /// <returns>The task</returns>
        public async Task ApplyUbiRayAsync()
        {
            try
            {
                RCFCore.Logger?.LogInformationSource($"The Rayman Legends UbiRay utility is being applied...");

                if (!SelectedPath.DirectoryExists)
                {
                    RCFCore.Logger?.LogInformationSource($"The Rayman Legends UbiRay utility could not be applied due to the selected directory not existing");

                    await RCFUI.MessageUI.DisplayMessageAsync(Resources.RLU_UbiRay_InvalidSaveDir, MessageType.Error);
                    return;
                }

                if (!await RCFUI.MessageUI.DisplayMessageAsync(Resources.RLU_UbiRay_Warning, "Important Information", MessageType.Warning, true))
                    return;

                FileSystemPath saveFilePath = SelectedPath + "RaymanSave_0";

                List<int> locations = new List<int>();

                var hex = File.ReadAllBytes(saveFilePath).Select(item => item.ToString("X2")).ToList();

                string[] username = BitConverter.ToString(Encoding.Default.GetBytes(SelectedPath.Name)).Split('-');

                int usernameLocation = 0;

                // Find the username location in the file since the bytes to change are always after that
                for (int i = 0; i < hex.Count - username.Length; i++)
                {
                    if (hex.GetRange(i, username.Length).ToArray().SequenceEqual(username))
                        usernameLocation = i;
                }

                if (usernameLocation == 0)
                {
                    RCFCore.Logger?.LogInformationSource($"The Rayman Legends UbiRay utility could not be applied due to the username location not being found");

                    await RCFUI.MessageUI.DisplayMessageAsync(Resources.RLU_UbiRay_CouldNotReadFile, MessageType.Error);
                    return;
                }

                // Find all occurrences
                for (int i = usernameLocation; i < hex.Count - 3; i++)
                {
                    if (hex[i] == "B2" && hex[i + 1] == "23" && hex[i + 2] == "CC" && hex[i + 3] == "E9")
                        locations.Add(i);
                }

                if (locations.Count < 2)
                {
                    RCFCore.Logger?.LogInformationSource($"The Rayman Legends UbiRay utility could not be applied due to the costume location not being found");

                    await RCFUI.MessageUI.DisplayMessageAsync(Resources.RLU_UbiRay_RaymanSkinNotFound, "Error", MessageType.Error);
                    return;
                }

                const string ubiRayValue = "18E0894D";
                byte[] input = Enumerable.Range(0, ubiRayValue.Length)
                    .Where(x => x % 2 == 0)
                    .Select(x => Convert.ToByte(ubiRayValue.Substring(x, 2), 16))
                    .ToArray();

                try
                {
                    File.Copy(saveFilePath, saveFilePath.FullPath + ".backup");
                }
                catch (Exception ex)
                {
                    ex.HandleError("Backing up RL save file");
                }

                // There are two locations in the save which need to be edited, the first and last
                using (Stream stream = File.Open(saveFilePath, FileMode.Open))
                {
                    stream.Position = locations[0];
                    stream.Write(input, 0, input.Length);

                    stream.Position = locations.Last();
                    stream.Write(input, 0, input.Length);
                }

                RCFCore.Logger?.LogInformationSource($"The Rayman Legends UbiRay utility has been applied");

                await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.RLU_UbiRay_Success);
            }
            catch (Exception ex)
            {
                ex.HandleError("Applying RL UbiRay patch");
                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex);
            }
        }

        #endregion
    }
}