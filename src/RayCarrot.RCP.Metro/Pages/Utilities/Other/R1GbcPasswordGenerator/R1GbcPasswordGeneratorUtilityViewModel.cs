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
            // TODO-LOC
            new("Spellbound Forest 2", 10, 1, new CageViewModel[]
            {
                new("Cage 1", 14, 6),
                new("Cage 2", 51, 7),
            }),
            new("Spellbound Forest 3", 11, 2, new CageViewModel[]
            {
                new("Cage 1", 36, 8),
            }),
            new("Spellbound Forest 4", 12, 3, new CageViewModel[]
            {
                new("Cage 1", 40, 9),
            }),
            new("Airy Tunes 1", 27, 4, new CageViewModel[]
            {
                new("Cage 1", 36, 30),
                new("Cage 2", 62, 31),
            }),
            new("Airy Tunes 2", 28, 5, new CageViewModel[]
            {
                new("Cage 1", 7, 32),
                new("Cage 2", 41, 33),
            }),
            new("Airy Tunes 3", 29, 6, new CageViewModel[]
            {
                new("Cage 1", 40, 34),
                new("Cage 2", 69, 35),
            }),
            new("Airy Tunes 4", 30, 7, new CageViewModel[]
            {
                new("Cage 1", 23, 36),
                new("Cage 2", 53, 37),
            }),
            new("Rainy Forest 1", 13, 8, new CageViewModel[]
            {
                new("Cage 1", 57, 10),
                new("Cage 2", 58, 11),
            }),
            new("Rainy Forest 2", 14, 9, new CageViewModel[]
            {
                new("Cage 1", 54, 12),
                new("Cage 2", 55, 13),
            }),
            new("Rainy Forest 3", 15, 10, new CageViewModel[]
            {
                new("Cage 1", 34, 14),
            }),
            new("Rocky Peaks 1", 26, 11, new CageViewModel[]
            {
                new("Cage 1", 18, 29),
            }),
            new("Rocky Peaks 2", 23, 12, new CageViewModel[]
            {
                new("Cage 1", 65, 24),
            }),
            new("Rocky Peaks 3", 24, 13, new CageViewModel[]
            {
                new("Cage 1", 51, 25),
                new("Cage 2", 61, 26),
            }),
            new("Rocky Peaks 4", 25, 14, new CageViewModel[]
            {
                new("Cage 1", 23, 27),
                new("Cage 2", 65, 28),
            }),
            new("Ancient Forest 1", 16, 15, new CageViewModel[]
            {
                new("Cage 1", 58, 15),
            }),
            new("Ancient Forest 2", 17, 16, new CageViewModel[]
            {
                new("Cage 1", 21, 16),
            }),
            new("Ancient Forest 3", 18, 17, new CageViewModel[]
            {
                new("Cage 1", 3, 17),
                new("Cage 2", 59, 18),
            }),
            new("Fiery Depths 1", 1, 18, new CageViewModel[]
            {
                new("Cage 1", 55, 2),
                new("Cage 2", 70, 3),
            }),
            new("Fiery Depths 2", 0, 19, new CageViewModel[]
            {
                new("Cage 1", 83, 0),
                new("Cage 2", 84, 1),
            }),
            new("Fiery Depths 3", 2, 20, new CageViewModel[]
            {
                new("Cage 1", 10, 4),
            }),
            new("Fiery Depths 4", 3, 21, new CageViewModel[]
            {
                new("Cage 1", 72, 5),
            }),
            new("Arcane Forest 1", 19, 22, new CageViewModel[]
            {
                new("Cage 1", 17, 19),
                new("Cage 2", 68, 20),
            }),
            new("Arcane Forest 2", 20, 23, new CageViewModel[]
            {
                new("Cage 1", 43, 21),
                new("Cage 2", 64, 22),
            }),
            new("Arcane Forest 4", 22, 25, new CageViewModel[]
            {
                new("Cage 1", 34, 23),
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

    public override LocalizedString DisplayHeader => "Rayman 1 GBC Password Generator"; // TODO-LOC
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