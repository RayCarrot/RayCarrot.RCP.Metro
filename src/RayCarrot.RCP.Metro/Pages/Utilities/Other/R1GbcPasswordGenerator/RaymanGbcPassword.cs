namespace RayCarrot.RCP.Metro.Pages.Utilities;

public class RaymanGbcPassword
{
    public RaymanGbcPassword()
    {
        SetCustomSwapMasks([0x15, 0x19, 0xC]);
    }

    private static char[] DisplayCharsTable { get; } = 
    [
        '-', 'B', 'C', 'D', 'F', 'G', 'H', 'J',
        'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S',
        'T', 'V', 'W', 'X', 'Y', 'Z', '0', '1',
        '2', '3', '4', '5', '6', '7', '8', '9',
        'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k',
        'l', 'm', 'n', 'p', 'q', 'r', 's', 't',
        'v', 'w', 'x', 'y', 'z', '?', '#', '/',
        '(', ')', '+', '!', '=', '*', '%', '^',
    ];

    // NOTE: This array should be 3 bytes long, but the game's code overflows it into the next array, using it as a 6-byte array
    private static byte[] OriginalSwapMasks { get; } = [0xa0, 0x40, 0xe0, 0x00, 0x09, 0x00]; // 5:5ef9
    private static byte[] PasswordSwapTable { get; } = // 5:5efc
    [
        0, 9, 
        0, 8, 
        0, 7, 
        2, 5, 
        2, 4, 
        2, 3, 
        1, 9, 
        1, 8, 
        1, 7,
        1, 6,
    ];
    private static byte[] XorKey { get; } = "VALERIOGBC"u8.ToArray(); // 5:5f10

    private byte[] CustomSwapMasks { get; } = new byte[3]; // c81d

    public byte[] SaveData { get; } = new byte[10]; // c80b
    public byte[] Password { get; } = new byte[10]; // c813

    // 5:6e50
    private byte GetTotalCagesInGame()
    {
        // The game gets this from the game data, but we can just hard-code it
        return 38;
    }

    // 5:7022
    private byte GetStoryLevelsCount()
    {
        // The game gets this from the game data, but we can just hard-code it
        return 27;
    }

    // 5:7047
    private byte GetPostGameLevelsCount()
    {
        // The game gets this from the game data, but we can just hard-code it
        return 5;
    }

    // 5:5e5f
    private void SetCustomSwapMasks(byte[] customSwapMasks)
    {
        Array.Copy(customSwapMasks, CustomSwapMasks, CustomSwapMasks.Length);
    }

    // 5:5cef
    private void Decrypt()
    {
        DecodeSwaps();
        XorAndMask();
    }

    // 5:5ce8
    private void Encrypt()
    {
        XorAndMask();
        EncodeSwaps();
    }

    // 5:5c70
    private void DecodeSwaps()
    {
        // Calculate key (always turns into 0x20 0x2C 0x18)
        byte[] swapMasks = new byte[3];
        for (int i = 0; i < 3; i++)
            swapMasks[i] = (byte)(((CustomSwapMasks[i] | OriginalSwapMasks[i * 2]) ^ 0x55) & 0x3F);

        // Perform swaps
        int swapMaskIndex = 1;
        int swapTableIndex = PasswordSwapTable.Length - 1;
        for (int j = 0; j < PasswordSwapTable.Length / 2; j++)
        {
            swapMaskIndex--;
            if (swapMaskIndex == -1)
                swapMaskIndex = 2;

            int index1 = PasswordSwapTable[swapTableIndex];
            swapTableIndex--;
            int index2 = PasswordSwapTable[swapTableIndex];
            swapTableIndex--;

            BitSwap(index1, index2, swapMasks[swapMaskIndex]);
        }
    }

    // 5:5bfd
    private void EncodeSwaps()
    {
        // Calculate key (always turns into 0x20 0x2C 0x18)
        byte[] swapMasks = new byte[3];
        for (int i = 0; i < 3; i++)
            swapMasks[i] = (byte)(((CustomSwapMasks[i] | OriginalSwapMasks[i * 2]) ^ 0x55) & 0x3F);

        // Perform swaps
        int swapMaskIndex = 2;
        int swapTableIndex = 0;
        for (int j = 0; j < PasswordSwapTable.Length / 2; j++)
        {
            swapMaskIndex++;
            if (swapMaskIndex == 3)
                swapMaskIndex = 0;

            int index1 = PasswordSwapTable[swapTableIndex];
            swapTableIndex++;
            int index2 = PasswordSwapTable[swapTableIndex];
            swapTableIndex++;

            BitSwap(index2, index1, swapMasks[swapMaskIndex]);
        }
    }

    // 5:5bbe
    private void BitSwap(int index1, int index2, int mask)
    {
        int v1 = Password[index1];
        int v2 = Password[index2];
        Password[index1] = (byte)(~mask & v1 | v2 & mask);
        Password[index2] = (byte)(~mask & v2 | v1 & mask);
    }

    // 5:5bb0
    private void XorAndMask()
    {
        XorAndMask(Password, XorKey, 0x3F);
    }

    // 5:58c3
    private void XorAndMask(byte[] data, byte[] xorKey, byte mask)
    {
        for (int i = 0; i < data.Length; i++)
            data[i] = (byte)(mask & (data[i] ^ xorKey[i]));
    }

    // 5:5d86
    private void ConvertPasswordToSaveData()
    {
        Array.Clear(SaveData, 0, SaveData.Length);

        // Convert 6-bit to 8-bit data
        int bitIndex = 0;
        foreach (byte byteValue in Password)
        {
            // Shift to the left (0011 1111 -> 1111 1100)
            byte shiftedByteValue = (byte)(byteValue << 2);

            // Enumerate every bit, from high to low
            for (int i = 0; i < 6; i++)
            {
                // Check highest bit
                byte isSet = (byte)(shiftedByteValue >> 7);

                // Shift to the next bit
                shiftedByteValue = (byte)(shiftedByteValue << 1);

                // Set or clear bit
                if (isSet != 0)
                    SaveData[bitIndex / 8] |= (byte)(1 << (bitIndex % 8));
                else
                    SaveData[bitIndex / 8] &= (byte)~(1 << (bitIndex % 8));

                bitIndex++;
            }
        }
    }

    // 5:5cf6
    private void ConvertSaveDataToPassword()
    {
        // Convert 8-bit to 6-bit data
        int bitIndex = 0;
        for (int i = 0; i < Password.Length; i++)
        {
            byte passwordByte = 0;

            for (int j = 0; j < 6; j++)
            {
                byte saveDataByte = SaveData[bitIndex / 8];

                passwordByte <<= 1;

                // Set bit
                if ((saveDataByte & (1 << (bitIndex % 8))) != 0)
                    passwordByte |= 1;

                bitIndex++;
            }

            Password[i] = passwordByte;
        }
    }

    // 5:5ba9
    private bool ValidateChecksum(byte expectedChecksum)
    {
        byte checksum = CalculateChecksum();
        return checksum == expectedChecksum;
    }

    // 5:5b86
    private byte CalculateChecksum()
    {
        byte checksum = 0;
        foreach (byte byteValue in SaveData)
            checksum += byteValue;
        byte checksumValue = GetChecksum();
        return (byte)(~(checksumValue - checksum) + 1);
    }

    // 5:5e30
    private void UpdateChecksum()
    {
        byte checksum = CalculateChecksum();
        SetChecksum(checksum);
    }

    // 5:5e37
    public bool Decode(byte[] password)
    {
        Array.Copy(password, Password, Password.Length);
        Decrypt();
        ConvertPasswordToSaveData();
        byte checksum = GetChecksum();
        return ValidateChecksum(checksum);
    }

    // 5:5d79
    public byte[] Encode()
    {
        UpdateChecksum();
        ConvertSaveDataToPassword();
        Encrypt();
        return Password;
    }

    // 6:680d
    public bool IsSaveDataValid()
    {
        byte level = GetLevel();
        byte storyLevelsCount = GetStoryLevelsCount();
        byte postGameLevelsCount = GetPostGameLevelsCount();
        byte totalCagesInGame = GetTotalCagesInGame();
        byte totalCollectedCages = GetTotalCollectedCages();
        byte livesCount = GetLivesCount();
        bool hasUnlockedWorldMap = GetHasUnlockedWorldMap();

        // Lives can't be 0
        if (livesCount == 0)
            return false;

        // Make sure the level isn't out of bounds
        if (level > storyLevelsCount + postGameLevelsCount + 1)
            return false;

        // If you're in the post-game then the worldmap has to be unlocked and all cages collected
        if (level > storyLevelsCount && 
            (!hasUnlockedWorldMap || totalCollectedCages != totalCagesInGame))
            return false;

        return true;
    }

    // 5:58fa
    public byte GetLivesCount()
    {
        return (byte)(SaveData[0] & 0x7f);
    }

    // 5:58d3
    public void SetLivesCount(byte lives)
    {
        SaveData[0] = (byte)(lives | SaveData[0] & 0x80);
    }

    // 5:5aff
    public bool GetHasUnlockedWorldMap()
    {
        return (SaveData[0] & 0x80) != 0;
    }

    // 5:5a85
    public void SetHasUnlockedWorldMap(bool unlocked)
    {
        if (unlocked)
            SaveData[0] |= 0x80;
        else
            SaveData[0] &= 0x7F;
    }

    // 5:5b3f
    private byte GetChecksum()
    {
        return SaveData[1];
    }

    // 5:5b60
    private void SetChecksum(byte checksum)
    {
        SaveData[1] = checksum;
    }

    // 5:5952
    public byte GetLevel()
    {
        return (byte)(SaveData[2] & 0x3F);
    }

    // 5:591c
    public void SetLevel(byte level)
    {
        SaveData[2] = (byte)(level | SaveData[2] & 0xc0);
    }

    // 5:59ec
    public bool GetHasCollectedCage(byte cageId)
    {
        int bitIndex = 22 + cageId;
        return (SaveData[bitIndex / 8] & (1 << (bitIndex % 8))) != 0;
    }

    // 5:5974
    public void SetHasCollectedCage(byte cageId, bool isCollected)
    {
        int bitIndex = 22 + cageId;
        if (isCollected)
            SaveData[bitIndex / 8] |= (byte)(1 << (bitIndex % 8));
        else
            SaveData[bitIndex / 8] &= (byte)~(1 << (bitIndex % 8));
    }

    // 5:5a2a
    public byte GetTotalCollectedCages()
    {
        byte count = 0;

        for (byte i = 0; i < GetTotalCagesInGame(); i++)
        {
            if (GetHasCollectedCage(i))
                count++;
        }

        return count;
    }

    public static byte[] GetPasswordFromString(string passwordString)
    {
        byte[] password = new byte[10];
        for (int i = 0; i < password.Length; i++)
            password[i] = (byte)Array.IndexOf(DisplayCharsTable, passwordString[i]);
        return password;
    }

    public static string GetStringFromPassword(byte[] password)
    {
        char[] str = new char[10];
        for (int i = 0; i < str.Length; i++)
            str[i] = DisplayCharsTable[password[i]];
        return new string(str);
    }

    public static char[] GetSupportedCharacters()
    {
        return DisplayCharsTable;
    }
}