using RayCarrot.Rayman;
using RayCarrot.Rayman.Ray1;
using RayCarrot.UI;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for generating Rayman 1 PS1 passwords
    /// </summary>
    public class R1PasswordGeneratorUtilityViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public R1PasswordGeneratorUtilityViewModel()
        {
            // Create commands
            GeneratePasswordCommand = new RelayCommand(GeneratePassword);
            LoadPasswordCommand = new AsyncRelayCommand(LoadPasswordAsync);

            // Set up selection
            ModeSelection = new EnumSelectionViewModel<R1_PS1_Password.PasswordMode>(R1_PS1_Password.PasswordMode.NTSC, new R1_PS1_Password.PasswordMode[]
            {
                R1_PS1_Password.PasswordMode.NTSC,
                //R1_PS1_Password.PasswordMode.PAL, // TODO: Support the PAL version
            });

            // Set levels
            Levels = new LevelViewModel[]
            {
                // TODO-UPDATE: Localize
                new LevelViewModel(new LocalizedString(() => "Pink Plant Woods"), ProcessUnlockedChange, canIsUnlockedBeModified: false),
                new LevelViewModel(new LocalizedString(() => "Anguish Lagoon"), ProcessUnlockedChange),
                new LevelViewModel(new LocalizedString(() => "The Swamps of Forgetfulness"), ProcessUnlockedChange, link: 4),
                new LevelViewModel(new LocalizedString(() => "Moskito's Nest"), ProcessUnlockedChange, branched: true, bossIndex: 1),
                new LevelViewModel(new LocalizedString(() => "Bongo Hills"), ProcessUnlockedChange),
                new LevelViewModel(new LocalizedString(() => "Allegro Presto"), ProcessUnlockedChange),
                new LevelViewModel(new LocalizedString(() => "Gong Heights"), ProcessUnlockedChange, link: 8),
                new LevelViewModel(new LocalizedString(() => "Mr Sax's Hullaballoo"), ProcessUnlockedChange, branched: true, bossIndex: 2),
                new LevelViewModel(new LocalizedString(() => "Twilight Gulch"), ProcessUnlockedChange),
                new LevelViewModel(new LocalizedString(() => "The Hard Rocks"), ProcessUnlockedChange),
                new LevelViewModel(new LocalizedString(() => "Mr Stone's Peaks"), ProcessUnlockedChange),
                new LevelViewModel(new LocalizedString(() => "Eraser Plains"), ProcessUnlockedChange),
                new LevelViewModel(new LocalizedString(() => "Pencil Pentathlon"), ProcessUnlockedChange),
                new LevelViewModel(new LocalizedString(() => "Space Mama's Crater"), ProcessUnlockedChange),
                new LevelViewModel(new LocalizedString(() => "Crystal Palace"), ProcessUnlockedChange),
                new LevelViewModel(new LocalizedString(() => "Eat at Joe's"), ProcessUnlockedChange),
                new LevelViewModel(new LocalizedString(() => "Mr Skops' Stalactites"), ProcessUnlockedChange, bossIndex: 6)
            };

            // First level should always be unlocked
            Levels[0].IsUnlocked = true;

            Password = "??????????";
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The game mode selection
        /// </summary>
        public EnumSelectionViewModel<R1_PS1_Password.PasswordMode> ModeSelection { get; }

        /// <summary>
        /// The game levels which can be modified
        /// </summary>
        public LevelViewModel[] Levels { get; }

        public bool HasHelpedTheMusician { get; set; }
        public bool HasFinishedTheGame { get; set; }

        public int LivesCount { get; set; }
        public int ContinuesCount { get; set; }

        public string Password { get; set; }

        #endregion

        #region Public Methods

        public void ProcessUnlockedChange(LevelViewModel changedLvl)
        {
            var disabled = !changedLvl.IsUnlocked;

            // No need to process branched levels
            if (changedLvl.Branched)
                return;

            bool reachedItem = false;

            // Update normal levels
            foreach (LevelViewModel lvl in Levels)
            {
                // Skip branched and linked levels
                if (lvl.Branched || lvl.Link != -1)
                    continue;

                if (reachedItem && disabled)
                    lvl.SetIsUnlocked(false);

                if (lvl == changedLvl)
                    reachedItem = true;

                if (!reachedItem && !disabled)
                    lvl.SetIsUnlocked(true);
            }

            // Update linked levels
            foreach (LevelViewModel lvl in Levels.Where(x => x.Link != -1))
                lvl.SetIsUnlocked(Levels[lvl.Link].IsUnlocked);
        }

        public void GeneratePassword()
        {
            // Create a save
            var save = new R1_PS1_Password.R1_PS1_SaveFile
            {
                LivesCount = (byte)LivesCount,
                Continues = (byte)ContinuesCount
            };

            // Set level states
            for (int i = 0; i < Levels.Length; i++)
            {
                save.WorldInfo[i].IsUnlocked = Levels[i].IsUnlocked;
                save.WorldInfo[i].HasAllCages = Levels[i].HasAllCages;
            }
            
            // Set boss flags
            foreach (var lev in Levels.Where(x => x.BossIndex != -1))
                save.FinBossLevel[0] = (byte)BitHelpers.SetBits(save.FinBossLevel[0], lev.BeatBoss ? 1 : 0, 1, lev.BossIndex);

            // Set flags
            save.FinBossLevel[0] = (byte)BitHelpers.SetBits(save.FinBossLevel[0], HasFinishedTheGame ? 1 : 0, 1, 7);
            save.FinBossLevel[1] = (byte)BitHelpers.SetBits(save.FinBossLevel[1], HasHelpedTheMusician ? 1 : 0, 1, 3);

            // Get the password
            var password = new R1_PS1_Password(save, ModeSelection.SelectedValue);

            Password = password.ToString().ToUpper();
        }

        public async Task LoadPasswordAsync()
        {
            // TODO-UPDATE: Verify password length and characters

            var password = new R1_PS1_Password(Password, ModeSelection.SelectedValue);
            var save = password.Decode();

            if (save == null)
            {
                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayMessageAsync("Invalid password!", MessageType.Error);

                return;
            }

            LivesCount = save.LivesCount;
            ContinuesCount = save.Continues;

            // Set level states
            for (int i = 0; i < Levels.Length; i++)
            {
                Levels[i].IsUnlocked = save.WorldInfo[i].IsUnlocked;
                Levels[i].HasAllCages = save.WorldInfo[i].HasAllCages;
            }

            // Set boss flags
            foreach (var lev in Levels.Where(x => x.BossIndex != -1))
                lev.BeatBoss = BitHelpers.ExtractBits(save.FinBossLevel[0], 1, lev.BossIndex) == 1;

            // Set flags
            HasFinishedTheGame = BitHelpers.ExtractBits(save.FinBossLevel[0], 1, 7) == 1;
            HasHelpedTheMusician = BitHelpers.ExtractBits(save.FinBossLevel[1], 1, 3) == 1;
        }

        #endregion

        #region Commands

        public ICommand GeneratePasswordCommand { get; }
        public ICommand LoadPasswordCommand { get; }

        #endregion

        #region Classes

        public class LevelViewModel : BaseRCPViewModel
        {
            public LevelViewModel(LocalizedString levelName, Action<LevelViewModel> onChangedIsUnlocked, bool branched = false, int link = -1, bool canIsUnlockedBeModified = true, int bossIndex = -1)
            {
                LevelName = levelName;
                OnChangedIsUnlocked = onChangedIsUnlocked;
                Branched = branched;
                Link = link;
                CanIsUnlockedBeModified = canIsUnlockedBeModified && link == -1;
                BossIndex = bossIndex;
            }

            private bool _isUnlocked;

            public LocalizedString LevelName { get; }
            public Action<LevelViewModel> OnChangedIsUnlocked { get; }
            public bool Branched { get; }
            public int Link { get; }
            public bool CanIsUnlockedBeModified { get; }
            public bool IsUnlocked
            {
                get => _isUnlocked;
                set
                {
                    _isUnlocked = value;
                    OnChangedIsUnlocked(this);
                }
            }
            public bool HasAllCages { get; set; }
            public int BossIndex { get; }
            public bool HasBoss => BossIndex != -1;
            public bool BeatBoss { get; set; }

            public void SetIsUnlocked(bool isUnlocked)
            {
                _isUnlocked = isUnlocked;
                OnPropertyChanged(nameof(IsUnlocked));

                if (!IsUnlocked)
                {
                    HasAllCages = false;
                    BeatBoss = false;
                }
            }
        }

        #endregion
    }
}