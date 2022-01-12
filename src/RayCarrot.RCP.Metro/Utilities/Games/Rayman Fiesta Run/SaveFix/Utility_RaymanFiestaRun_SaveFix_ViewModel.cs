using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BinarySerializer;
using BinarySerializer.UbiArt;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the Rayman Fiesta Run save fix utility
/// </summary>
public class Utility_RaymanFiestaRun_SaveFix_ViewModel : BaseRCPViewModel
{
    #region Constructor

    public Utility_RaymanFiestaRun_SaveFix_ViewModel()
    {
        Editions = new ObservableCollection<EditionViewModel>();

        foreach (UserData_FiestaRunEdition edition in EnumHelpers.GetValues<UserData_FiestaRunEdition>())
        {
            FileSystemPath saveDir = Environment.SpecialFolder.LocalApplicationData.GetFolderPath() +
                                     "Packages" +
                                     Games.RaymanFiestaRun.GetManager<GameManager_RaymanFiestaRun_WinStore>(GameType.WinStore).GetFiestaRunFullPackageName(edition) +
                                     "LocalState";
            FileSystemPath saveFile = saveDir + (edition == UserData_FiestaRunEdition.Win10 ? "slot0.dat" : "slot1.dat");

            if (saveFile.FileExists)
                Editions.Add(new EditionViewModel(edition, saveFile));
        }

        SelectedEdition = Editions.FirstOrDefault();
    }

    #endregion

    #region Public Properties

    public ObservableCollection<EditionViewModel> Editions { get; }
    public EditionViewModel? SelectedEdition { get; set; }

    #endregion

    #region Classes

    public class EditionViewModel : BaseViewModel
    {
        public EditionViewModel(UserData_FiestaRunEdition edition, FileSystemPath saveFilePath)
        {
            Edition = edition;
            DisplayName = edition switch
            {
                UserData_FiestaRunEdition.Default => new ResourceLocString(nameof(Resources.FiestaRunVersion_Default)),
                UserData_FiestaRunEdition.Preload => new ResourceLocString(nameof(Resources.FiestaRunVersion_Preload)),
                UserData_FiestaRunEdition.Win10 => new ResourceLocString(nameof(Resources.FiestaRunVersion_Win10)),
                _ => throw new ArgumentOutOfRangeException(nameof(edition), edition, null)
            };
            SaveFilePath = saveFilePath;

            RequiresFixing = CheckIfSaveRequiresFix();

            FixCommand = new AsyncRelayCommand(FixAsync);
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ICommand FixCommand { get; }

        public UserData_FiestaRunEdition Edition { get; }
        public LocalizedString DisplayName { get; }
        public FileSystemPath SaveFilePath { get; }
        public bool RequiresFixing { get; set; }

        public bool CheckIfSaveRequiresFix()
        {
            try
            {
                using RCPContext context = new(SaveFilePath.Parent);
                context.AddSettings(new UbiArtSettings(Game.RaymanFiestaRun, Platform.PC));

                FiestaRun_SaveData? save = context.ReadFileData<FiestaRun_SaveData>(SaveFilePath.Name);

                if (save == null)
                    throw new Exception("Failed to load save data");

                IEnumerable<FiestaRun_SaveDataLevel> levels = save.LevelInfos_Land1;

                if (save.LevelInfos_Land2 != null)
                    levels = levels.Concat(save.LevelInfos_Land2);

                return levels.Any(lvl => lvl.HasCrown && lvl.Electoons < 4);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Checking Fiesta Run save");

                // Return false if there was an error since we can't fix it then anyway
                return false;
            }
        }

        public async Task FixAsync()
        {
            if (!RequiresFixing)
                return;

            try
            {
                using RCPContext context = new(SaveFilePath.Parent);
                context.AddSettings(new UbiArtSettings(Game.RaymanFiestaRun, Platform.PC));

                FiestaRun_SaveData? save = context.ReadFileData<FiestaRun_SaveData>(SaveFilePath.Name, removeFileWhenComplete: false);

                if (save == null)
                    throw new Exception("Failed to load save data");

                IEnumerable<FiestaRun_SaveDataLevel> levels = save.LevelInfos_Land1;

                if (save.LevelInfos_Land2 != null)
                    levels = levels.Concat(save.LevelInfos_Land2);

                foreach (FiestaRun_SaveDataLevel lvl in levels)
                {
                    if (lvl.HasCrown && lvl.Electoons < 4)
                        lvl.Electoons = 4;
                }

                FileFactory.Write(context, SaveFilePath.Name, save);

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplaySuccessfulActionMessageAsync("The save was successfully fixes");

                RequiresFixing = false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Fixing Fiesta Run save");

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when fixing the save");
            }
        }
    }

    #endregion
}