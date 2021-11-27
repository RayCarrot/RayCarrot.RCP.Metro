#nullable disable
using NLog;
using RayCarrot.Rayman.Ray1;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for generating Rayman 1 PS1 passwords
/// </summary>
public class Utility_R1PasswordGenerator_ViewModel : BaseRCPViewModel, IDisposable
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_R1PasswordGenerator_ViewModel()
    {
        // Create commands
        GeneratePasswordCommand = new AsyncRelayCommand(GeneratePasswordAsync);
        LoadPasswordCommand = new AsyncRelayCommand(LoadPasswordAsync);

        // Set up selection
        ModeSelection = new EnumSelectionViewModel<Rayman1PS1Password.PasswordMode>(Rayman1PS1Password.PasswordMode.NTSC, new Rayman1PS1Password.PasswordMode[]
        {
            Rayman1PS1Password.PasswordMode.NTSC,
            //R1_PS1_Password.PasswordMode.PAL, // TODO: Support the PAL version
        });

        // Set levels
        Levels = new LevelViewModel[]
        {
            new LevelViewModel(new ResourceLocString(nameof(Resources.R1_LevelName_0)), ProcessUnlockedChange, canIsUnlockedBeModified: false),
            new LevelViewModel(new ResourceLocString(nameof(Resources.R1_LevelName_1)), ProcessUnlockedChange),
            new LevelViewModel(new ResourceLocString(nameof(Resources.R1_LevelName_2)), ProcessUnlockedChange, link: 4),
            new LevelViewModel(new ResourceLocString(nameof(Resources.R1_LevelName_3)), ProcessUnlockedChange, branched: true, bossFlag: Rayman1FinBossLevelFlags.Moskito),
            new LevelViewModel(new ResourceLocString(nameof(Resources.R1_LevelName_4)), ProcessUnlockedChange),
            new LevelViewModel(new ResourceLocString(nameof(Resources.R1_LevelName_5)), ProcessUnlockedChange),
            new LevelViewModel(new ResourceLocString(nameof(Resources.R1_LevelName_6)), ProcessUnlockedChange, link: 8),
            new LevelViewModel(new ResourceLocString(nameof(Resources.R1_LevelName_7)), ProcessUnlockedChange, branched: true, bossFlag: Rayman1FinBossLevelFlags.MrSax),
            new LevelViewModel(new ResourceLocString(nameof(Resources.R1_LevelName_8)), ProcessUnlockedChange),
            new LevelViewModel(new ResourceLocString(nameof(Resources.R1_LevelName_9)), ProcessUnlockedChange),
            new LevelViewModel(new ResourceLocString(nameof(Resources.R1_LevelName_10)), ProcessUnlockedChange),
            new LevelViewModel(new ResourceLocString(nameof(Resources.R1_LevelName_11)), ProcessUnlockedChange),
            new LevelViewModel(new ResourceLocString(nameof(Resources.R1_LevelName_12)), ProcessUnlockedChange),
            new LevelViewModel(new ResourceLocString(nameof(Resources.R1_LevelName_13)), ProcessUnlockedChange),
            new LevelViewModel(new ResourceLocString(nameof(Resources.R1_LevelName_14)), ProcessUnlockedChange),
            new LevelViewModel(new ResourceLocString(nameof(Resources.R1_LevelName_15)), ProcessUnlockedChange),
            new LevelViewModel(new ResourceLocString(nameof(Resources.R1_LevelName_16)), ProcessUnlockedChange, bossFlag: Rayman1FinBossLevelFlags.MrSkops),
            new LevelViewModel(new ResourceLocString(nameof(Resources.R1_LevelName_17)), ProcessUnlockedChange, bossFlag: Rayman1FinBossLevelFlags.MrDark, hasCages: false)
        };

        // First level should always be unlocked
        Levels[0].IsUnlocked = true;

        Password = "??????????";
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    /// <summary>
    /// The game mode selection
    /// </summary>
    public EnumSelectionViewModel<Rayman1PS1Password.PasswordMode> ModeSelection { get; }

    /// <summary>
    /// The game levels which can be modified
    /// </summary>
    public LevelViewModel[] Levels { get; }

    public bool HasHelpedTheMusician { get; set; }

    public int LivesCount { get; set; }
    public int ContinuesCount { get; set; }

    public string Password { get; set; }

    #endregion

    #region Public Methods

    public void ProcessUnlockedChange(LevelViewModel changedLvl)
    {
        var disabled = !changedLvl.IsUnlocked;

        // If the level is branched we need to unlock the previous levels
        if (changedLvl.Branched)
        {
            var prevLev = Levels[Levels.FindItemIndex(x => x == changedLvl) - 1];
            Levels[prevLev.Link].IsUnlocked = true;
            return;
        }

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
        {
            var isUnlocked = Levels[lvl.Link].IsUnlocked;
            lvl.SetIsUnlocked(isUnlocked);

            // If we lock a linked level we also have to lock the next level which is always the branched one
            if (!isUnlocked)
                Levels[Levels.FindItemIndex(x => x == lvl) + 1].SetIsUnlocked(false);
        }
    }

    public void CorrectData()
    {
        // Clamp lives and continues
        LivesCount = LivesCount.Clamp(0, 99);
        ContinuesCount = ContinuesCount.Clamp(0, 9);

        // Moskito has to be beaten before finishing Twilight Gulch
        if (!Levels[3].BeatBoss && Levels[9].IsUnlocked)
        {
            Levels[3].IsUnlocked = true;
            Levels[3].BeatBoss = true;
        }

        // Make sure The Musician has been helped under valid circumstances
        if (HasHelpedTheMusician && !Levels[9].IsUnlocked)
            HasHelpedTheMusician = false;
        else if (!HasHelpedTheMusician && Levels[11].IsUnlocked)
            HasHelpedTheMusician = true;

        // Mr Skops has to be beaten before unlocking Mr Dark's Dare
        if (!Levels[16].BeatBoss && Levels[17].IsUnlocked)
            Levels[16].BeatBoss = true;

        // All cages have to be collected for Mr Dark's Dare to be unlocked
        if (Levels[17].IsUnlocked && Levels.Any(x => x.HasCages && !x.HasAllCages))
            Levels[17].IsUnlocked = false;
    }

    public async Task GeneratePasswordAsync()
    {
        // Correct the data in case it's out of range
        CorrectData();

        // Create a save
        var save = new Rayman1PS1Password.SaveData
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
        foreach (var lev in Levels.Where(x => x.HasBoss))
            save.FinBossLevel = save.FinBossLevel.SetFlag(lev.BossFlag, lev.BeatBoss);

        // Set flags
        save.FinBossLevel = save.FinBossLevel.SetFlag(Rayman1FinBossLevelFlags.HelpedMusician, HasHelpedTheMusician);

        // Validate the password
        var error = save.Validate();

        if (error != null)
        {
            Logger.Warn("Invalid R1 password: {0}", error);
            await Services.MessageUI.DisplayMessageAsync(Resources.R1Passwords_InvalidData, MessageType.Error);
            return;
        }

        // Get the password
        var password = new Rayman1PS1Password(save, ModeSelection.SelectedValue);

        Password = password.ToString().ToUpper();
    }

    public async Task LoadPasswordAsync()
    {
        var validationRule = new Utility_R1PasswordGenerator_PasswordValidationRule().Validate(Password, CultureInfo.CurrentCulture);

        if (!validationRule.IsValid)
            return;

        var password = new Rayman1PS1Password(Password, ModeSelection.SelectedValue);
        var save = password.Decode();

        if (save == null)
        {
            await Services.MessageUI.DisplayMessageAsync(Resources.R1Passwords_Invalid, MessageType.Error);
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
        foreach (var lev in Levels.Where(x => x.HasBoss))
            lev.BeatBoss = save.FinBossLevel.HasFlag(lev.BossFlag);

        // Set flags
        HasHelpedTheMusician = save.FinBossLevel.HasFlag(Rayman1FinBossLevelFlags.HelpedMusician);
    }

    #endregion

    #region Commands

    public ICommand GeneratePasswordCommand { get; }
    public ICommand LoadPasswordCommand { get; }

    #endregion

    #region Classes

    public class LevelViewModel : BaseRCPViewModel, IDisposable
    {
        public LevelViewModel(LocalizedString levelName, Action<LevelViewModel> onChangedIsUnlocked, bool branched = false, int link = -1, bool canIsUnlockedBeModified = true, Rayman1FinBossLevelFlags bossFlag = Rayman1FinBossLevelFlags.None, bool hasCages = true)
        {
            LevelName = levelName;
            OnChangedIsUnlocked = onChangedIsUnlocked;
            Branched = branched;
            Link = link;
            CanIsUnlockedBeModified = canIsUnlockedBeModified && link == -1;
            BossFlag = bossFlag;
            HasCages = hasCages;
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

                if (!IsUnlocked)
                {
                    HasAllCages = false;
                    BeatBoss = false;
                }

                OnChangedIsUnlocked(this);
            }
        }
        public bool HasCages { get; }
        public bool HasAllCages { get; set; }
        public Rayman1FinBossLevelFlags BossFlag { get; }
        public bool HasBoss => BossFlag != Rayman1FinBossLevelFlags.None;
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

        public void Dispose()
        {
            LevelName?.Dispose();
        }
    }

    #endregion

    public void Dispose()
    {
        Levels?.DisposeAll();
    }
}