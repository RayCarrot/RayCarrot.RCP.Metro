using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Windows.Management.Deployment;
using MahApps.Metro.IconPacks;
using Microsoft.Win32;
using RayCarrot.CarrotFramework;
using RayCarrot.Windows.Registry;
using RayCarrot.Windows.Shell;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Extension methods for <see cref="GameType"/>
    /// </summary>
    public static class GameTypeExtensions
    {
        /// <summary>
        /// Gets the display name for the specified game type
        /// </summary>
        /// <param name="gameType">The type of game to get the display name for</param>
        /// <returns>The display name</returns>
        public static string GetDisplayName(this GameType gameType)
        {
            switch (gameType)
            {
                case GameType.Win32:
                    return "Desktop";

                case GameType.Steam:
                    return "Steam";

                case GameType.WinStore:
                    return "Windows Store";

                case GameType.DosBox:
                    return "DosBox";

                default:
                    throw new ArgumentOutOfRangeException(nameof(gameType), gameType, null);
            }
        }
    }
}