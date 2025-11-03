using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Pages.Utilities;

/// <summary>
/// View model for generating Rayman 1 GBC passwords
/// </summary>
public class R1GbcPasswordGeneratorUtilityViewModel : UtilityViewModel
{
    #region Constructor

    public R1GbcPasswordGeneratorUtilityViewModel()
    {
        // Set levels
        Levels = new LevelViewModel[]
        {
            new(new ResourceLocString(nameof(Resources.R1GBC_LevelName_1)), 10, 1, new CageViewModel[]
            {
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage1)), 14, 6),
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage2)), 51, 7),
            }),
            new(new ResourceLocString(nameof(Resources.R1GBC_LevelName_2)), 11, 2, new CageViewModel[]
            {
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage1)), 36, 8),
            }),
            new(new ResourceLocString(nameof(Resources.R1GBC_LevelName_3)), 12, 3, new CageViewModel[]
            {
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage1)), 40, 9),
            }),
            new(new ResourceLocString(nameof(Resources.R1GBC_LevelName_4)), 27, 4, new CageViewModel[]
            {
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage1)), 36, 30),
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage2)), 62, 31),
            }),
            new(new ResourceLocString(nameof(Resources.R1GBC_LevelName_5)), 28, 5, new CageViewModel[]
            {
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage1)), 7, 32),
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage2)), 41, 33),
            }),
            new(new ResourceLocString(nameof(Resources.R1GBC_LevelName_6)), 29, 6, new CageViewModel[]
            {
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage1)), 40, 34),
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage2)), 69, 35),
            }),
            new(new ResourceLocString(nameof(Resources.R1GBC_LevelName_7)), 30, 7, new CageViewModel[]
            {
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage1)), 23, 36),
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage2)), 53, 37),
            }),
            new(new ResourceLocString(nameof(Resources.R1GBC_LevelName_8)), 13, 8, new CageViewModel[]
            {
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage1)), 57, 10),
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage2)), 58, 11),
            }),
            new(new ResourceLocString(nameof(Resources.R1GBC_LevelName_9)), 14, 9, new CageViewModel[]
            {
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage1)), 54, 12),
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage2)), 55, 13),
            }),
            new(new ResourceLocString(nameof(Resources.R1GBC_LevelName_10)), 15, 10, new CageViewModel[]
            {
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage1)), 34, 14),
            }),
            new(new ResourceLocString(nameof(Resources.R1GBC_LevelName_11)), 26, 11, new CageViewModel[]
            {
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage1)), 18, 29),
            }),
            new(new ResourceLocString(nameof(Resources.R1GBC_LevelName_12)), 23, 12, new CageViewModel[]
            {
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage1)), 65, 24),
            }),
            new(new ResourceLocString(nameof(Resources.R1GBC_LevelName_13)), 24, 13, new CageViewModel[]
            {
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage1)), 51, 25),
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage2)), 61, 26),
            }),
            new(new ResourceLocString(nameof(Resources.R1GBC_LevelName_14)), 25, 14, new CageViewModel[]
            {
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage1)), 23, 27),
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage2)), 65, 28),
            }),
            new(new ResourceLocString(nameof(Resources.R1GBC_LevelName_15)), 16, 15, new CageViewModel[]
            {
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage1)), 58, 15),
            }),
            new(new ResourceLocString(nameof(Resources.R1GBC_LevelName_16)), 17, 16, new CageViewModel[]
            {
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage1)), 21, 16),
            }),
            new(new ResourceLocString(nameof(Resources.R1GBC_LevelName_17)), 18, 17, new CageViewModel[]
            {
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage1)), 3, 17),
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage2)), 59, 18),
            }),
            new(new ResourceLocString(nameof(Resources.R1GBC_LevelName_18)), 1, 18, new CageViewModel[]
            {
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage1)), 55, 2),
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage2)), 70, 3),
            }),
            new(new ResourceLocString(nameof(Resources.R1GBC_LevelName_19)), 0, 19, new CageViewModel[]
            {
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage1)), 83, 0),
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage2)), 84, 1),
            }),
            new(new ResourceLocString(nameof(Resources.R1GBC_LevelName_20)), 2, 20, new CageViewModel[]
            {
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage1)), 10, 4),
            }),
            new(new ResourceLocString(nameof(Resources.R1GBC_LevelName_21)), 3, 21, new CageViewModel[]
            {
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage1)), 72, 5),
            }),
            new(new ResourceLocString(nameof(Resources.R1GBC_LevelName_22)), 19, 22, new CageViewModel[]
            {
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage1)), 17, 19),
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage2)), 68, 20),
            }),
            new(new ResourceLocString(nameof(Resources.R1GBC_LevelName_23)), 20, 23, new CageViewModel[]
            {
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage1)), 43, 21),
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage2)), 64, 22),
            }),
            new(new ResourceLocString(nameof(Resources.R1GBC_LevelName_25)), 22, 25, new CageViewModel[]
            {
                new(new ResourceLocString(nameof(Resources.R1GBCPasswords_Cage1)), 34, 23),
            }),
        };

        // Set initial password
        UnlockedWorldMap = false;
        LivesCount = 3;
        Level = 0;
        Password = "CG-G8LSJsD";

        // Create commands
        GeneratePasswordCommand = new AsyncRelayCommand(GeneratePasswordAsync);
        LoadPasswordCommand = new AsyncRelayCommand(LoadPasswordAsync);
    }

    #endregion

    #region Commands

    public ICommand GeneratePasswordCommand { get; }
    public ICommand LoadPasswordCommand { get; }

    #endregion

    #region Public Properties

    public override LocalizedString DisplayHeader => new ResourceLocString(nameof(Resources.Utilities_R1GbcPasswordGenerator_Header));
    public override GenericIconKind Icon => GenericIconKind.Utilities_R1PasswordGenerator;

    public LevelViewModel[] Levels { get; }

    public bool UnlockedWorldMap { get; set; }
    public int LivesCount { get; set; }
    public int Level { get; set; }

    public string Password { get; set; }

    #endregion

    #region Public Methods

    public void CorrectData()
    {
        // Clamp lives and level
        LivesCount = LivesCount.Clamp(1, 127);
        Level = Level.Clamp(0, 27 + 5 + 1);

        // If in post-game then we have to unlock the worldmap and set all cages as collected
        if (Level > 27)
        {
            UnlockedWorldMap = true;
            foreach (LevelViewModel level in Levels)
            {
                foreach (CageViewModel cage in level.Cages)
                {
                    cage.IsCollected = true;
                }
            }
        }
    }

    public async Task GeneratePasswordAsync()
    {
        // Correct the data in case it's out of range
        CorrectData();

        RaymanGbcPassword password = new();

        foreach (LevelViewModel level in Levels)
        {
            foreach (CageViewModel cage in level.Cages)
            {
                password.SetHasCollectedCage(cage.GlobalId, cage.IsCollected);
            }
        }

        password.SetHasUnlockedWorldMap(UnlockedWorldMap);
        password.SetLivesCount((byte)LivesCount);
        password.SetLevel((byte)Level);

        // Validate the password
        bool isValid = password.IsSaveDataValid();

        if (!isValid)
        {
            await Services.MessageUI.DisplayMessageAsync(Resources.R1Passwords_InvalidData, MessageType.Error);
            return;
        }

        byte[] passwordData = password.Encode();
        Password = RaymanGbcPassword.GetStringFromPassword(passwordData);
    }

    public async Task LoadPasswordAsync()
    {
        R1GbcPasswordGeneratorUtilityPasswordValidationRule validationRule = new();
        ValidationResult validationResult = validationRule.Validate(Password, CultureInfo.CurrentCulture);

        if (!validationResult.IsValid)
            return;

        RaymanGbcPassword password = new();
        bool validChecksum = password.Decode(RaymanGbcPassword.GetPasswordFromString(Password));

        if (!validChecksum || !password.IsSaveDataValid())
        {
            await Services.MessageUI.DisplayMessageAsync(Resources.R1Passwords_Invalid, MessageType.Error);
            return;
        }

        foreach (LevelViewModel level in Levels)
        {
            foreach (CageViewModel cage in level.Cages)
            {
                cage.IsCollected = password.GetHasCollectedCage(cage.GlobalId);
            }
        }

        UnlockedWorldMap = password.GetHasUnlockedWorldMap();
        LivesCount = password.GetLivesCount();
        Level = password.GetLevel();
    }

    public override void Dispose()
    {
        base.Dispose();
        Levels.DisposeAll();
    }

    #endregion

    #region Classes

    public class LevelViewModel : BaseRCPViewModel, IDisposable
    {
        public LevelViewModel(LocalizedString levelName, int id, int adventureIndex, CageViewModel[] cages)
        {
            LevelName = levelName;
            Id = id;
            AdventureIndex = adventureIndex;
            Cages = cages;
        }

        public LocalizedString LevelName { get; }
        public int Id { get; }
        public int AdventureIndex { get; }
        public CageViewModel[] Cages { get; }

        public void Dispose()
        {
            LevelName.Dispose();
            Cages.DisposeAll();
        }
    }

    public class CageViewModel : BaseRCPViewModel, IDisposable
    {
        public CageViewModel(LocalizedString displayName, int xlateId, byte globalId)
        {
            DisplayName = displayName;
            XlateId = xlateId;
            GlobalId = globalId;
        }

        public LocalizedString DisplayName { get; }
        public int XlateId { get; }
        public byte GlobalId { get; }

        public bool IsCollected { get; set; }

        public void Dispose()
        {
            DisplayName.Dispose();
        }
    }

    #endregion
}