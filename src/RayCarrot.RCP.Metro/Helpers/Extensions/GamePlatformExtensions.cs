namespace RayCarrot.RCP.Metro;

public static class GamePlatformExtensions
{
    public static bool HasPlatformFlag(this GamePlatform gamePlatform, GamePlatformFlag flag) => 
        ((int)gamePlatform & (int)flag) != 0;
}