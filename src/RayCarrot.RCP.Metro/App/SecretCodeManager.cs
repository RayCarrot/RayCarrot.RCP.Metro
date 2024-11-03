using Nito.AsyncEx;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro.Controls;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Manages secret codes for this application
/// </summary>
public static class SecretCodeManager
{
    #region Static Constructor

    static SecretCodeManager()
    {
        AsyncLock = new AsyncLock();

        Codes = new Dictionary<Key[], Func<Task>>()
        {
            {
                // Konami code
                new Key[]
                {
                    Key.Up,
                    Key.Up,
                    Key.Down,
                    Key.Down,
                    Key.Left,
                    Key.Right,
                    Key.Left,
                    Key.Right,
                    Key.B,
                    Key.A
                },
                async () =>
                {
                    Application.Current.SetTheme(Services.Data.Theme_DarkMode, false, Color.FromRgb(0xf4, 0x43, 0x36)); // RedPrimary500

                    await Services.MessageUI.DisplayMessageAsync(Resources.SecretCodes_Konami, Resources.SecretCodes_KonamiHeader, MessageType.Success);
                }
            },
            {
                // RayCarrot code
                new Key[]
                {
                    Key.R,
                    Key.A,
                    Key.Y,
                    Key.C,
                    Key.A,
                    Key.R,
                    Key.R,
                    Key.O,
                    Key.T,
                },
                async () =>
                {
                    Application.Current.SetTheme(Services.Data.Theme_DarkMode, false, Color.FromRgb(0xff, 0x98, 0x00)); // OrangePrimary500

                    await Services.MessageUI.DisplayMessageAsync(Resources.SecretCodes_RayCarrot, Resources.SecretCodes_RayCarrotHeader, MessageType.Success);
                }
            },
            {
                // Lime code
                new Key[]
                {
                    Key.S,
                    Key.O,
                    Key.U,
                    Key.R,
                },
                async () =>
                {
                    Application.Current.SetTheme(Services.Data.Theme_DarkMode, false, Color.FromRgb(0xcd, 0xdc, 0x39)); // LimePrimary500

                    await Services.MessageUI.DisplayMessageAsync(Resources.SecretCodes_Lime, Resources.SecretCodes_LimeHeader, MessageType.Success);
                }
            },
            {
                // Cooking code
                new Key[]
                {
                    Key.C,
                    Key.O,
                    Key.O,
                    Key.K,
                    Key.I,
                    Key.N,
                    Key.G,
                },
                async () =>
                {
                    foreach (Image img in Application.Current.MainWindow.FindChildren<Image>(true))
                        img.Source = (ImageSource)new ImageSourceConverter().ConvertFrom(SecretCodeAsset.Cooking.GetAssetPath());

                    await Services.MessageUI.DisplayMessageAsync(Resources.SecretCodes_Cooking, Resources.SecretCodes_CookingHeader, MessageType.Success);
                }
            },
            {
                // Secret code
                new Key[]
                {
                    Key.S,
                    Key.E,
                    Key.C,
                    Key.R,
                    Key.E,
                    Key.T,
                },
                async () =>
                {
                    await Services.MessageUI.DisplayMessageAsync(Resources.SecretCodes_Secret, Resources.SecretCodes_SecretHeader, MessageType.Success);
                }
            },
        };

        CurrentInput = new List<Key>(Codes.OrderBy(x => x.Key.Length).First().Key.Length);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Static Methods

    /// <summary>
    /// Add a new key to the manager
    /// </summary>
    /// <param name="key">The key to add</param>
    /// <returns>The task</returns>
    public static async Task AddKeyAsync(Key key)
    {
        // Lock the method
        using (await AsyncLock.LockAsync())
        {
            // Check if it's the following key in any of the available codes
            if (Codes.All(x => x.Key.Length <= CurrentInput.Count || x.Key[CurrentInput.Count] != key))
            {
                if (CurrentInput.Any())
                {
                    CurrentInput.Clear();
                    Logger.Debug("The secret code inputs were reset due to an invalid key being pressed");
                }

                return;
            }

            // Add the key to the list
            CurrentInput.Add(key);

            // Attempt to get a completed code
            var task = Codes.FirstOrDefault(x => x.Key.SequenceEqual(CurrentInput)).Value;

            if (task == null)
                return;

            CurrentInput.Clear();
            Logger.Debug("The secret code inputs were reset due to a valid code having been entered");

            // Run the task
            await task();
        }
    }

    #endregion

    #region Private Static Properties

    /// <summary>
    /// The async lock for adding a new key
    /// </summary>
    private static AsyncLock AsyncLock { get; }

    /// <summary>
    /// The currently saved input
    /// </summary>
    private static List<Key> CurrentInput { get; }

    /// <summary>
    /// The available codes and their tasks
    /// </summary>
    private static Dictionary<Key[], Func<Task>> Codes { get; }

    #endregion
}