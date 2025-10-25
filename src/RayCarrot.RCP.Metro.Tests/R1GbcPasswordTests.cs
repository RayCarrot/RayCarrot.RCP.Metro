using RayCarrot.RCP.Metro.Pages.Utilities;

namespace RayCarrot.RCP.Metro.Tests;

public class R1GbcPasswordTests
{
    [Fact]
    public void DecodePasswords()
    {
        RaymanGbcPassword password = new();

        Assert.True(password.Decode(RaymanGbcPassword.GetPasswordFromString("CG-G8LSJsD")));
        Assert.True(password.IsSaveDataValid());
        Assert.Equal(3, password.GetLivesCount());
        Assert.Equal(0, password.GetLevel());
        Assert.Equal(0, password.GetTotalCollectedCages());
        Assert.False(password.GetHasUnlockedWorldMap());

        // TODO: Add more
    }

    [Fact]
    public void EncodePasswords()
    {
        RaymanGbcPassword password = new();

        password.SetLivesCount(3);
        password.SetLevel(0);
        for (byte i = 0; i < 38; i++)
            password.SetHasCollectedCage(i, false);
        password.SetHasUnlockedWorldMap(false);
        Assert.Equal("CG-G8LSJsD", RaymanGbcPassword.GetStringFromPassword(password.Encode()));

        password.SetLivesCount(3);
        password.SetLevel(1);
        for (byte i = 0; i < 38; i++)
            password.SetHasCollectedCage(i, false);
        password.SetHasUnlockedWorldMap(false);
        Assert.Equal("CJCG8LSJdD", RaymanGbcPassword.GetStringFromPassword(password.Encode()));

        // TODO: Add more
    }
}