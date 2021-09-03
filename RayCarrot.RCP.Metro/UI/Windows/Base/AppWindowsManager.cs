using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using RayCarrot.Common;
using RayCarrot.Logging;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Manager for application windows
    /// </summary>
    public static class AppWindowsManager
    {
        #region Constructor

        static AppWindowsManager()
        {
            Windows = new Dictionary<WeakReference<Window>, WindowFlagModel>();
        }

        #endregion

        #region Private Static Properties

        /// <summary>
        /// The windows shown from <see cref="ShowWindow{Win}(Func{Win},ShowWindowFlags,String[])"/>
        /// </summary>
        private static Dictionary<WeakReference<Window>, WindowFlagModel> Windows { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Shows the specified <see cref="Window"/>
        /// </summary>
        /// <typeparam name="Win">The type of <see cref="Window"/> to show</typeparam>
        /// <param name="flags">The flags</param>
        /// <param name="groupNames">The group names for this instance</param>
        /// <returns>True if the <see cref="Window"/> is showing or false if it is not</returns>
        public static bool ShowWindow<Win>(ShowWindowFlags flags = ShowWindowFlags.None, params string[] groupNames)
            where Win : Window
        {
            return ShowWindow(typeof(Win).CreateInstance<Win>, flags, groupNames);
        }

        /// <summary>
        /// Shows the specified <see cref="Window"/>
        /// </summary>
        /// <typeparam name="Win">The type of <see cref="Window"/> to show</typeparam>
        /// <param name="createWindowFunc">The func for creating a new instance of the <see cref="Window"/></param>
        /// <param name="flags">The flags</param>
        /// <param name="groupNames">The group names for this instance</param>
        /// <returns>True if the <see cref="Window"/> is showing or false if it is not</returns>
        public static bool ShowWindow<Win>(Func<Win> createWindowFunc, ShowWindowFlags flags = ShowWindowFlags.None, params string[] groupNames)
            where Win : Window
        {
            return ShowWindow(createWindowFunc, typeof(Win), flags, groupNames);
        }

        /// <summary>
        /// Shows the specified <see cref="Window"/>
        /// </summary>
        /// <param name="window">The window to show</param>
        /// <param name="flags">The flags</param>
        /// <param name="groupNames">The group names for this instance</param>
        /// <returns>True if the <see cref="Window"/> is showing or false if it is not</returns>
        public static bool ShowWindow(Window window, ShowWindowFlags flags = ShowWindowFlags.None, params string[] groupNames)
        {
            return ShowWindow(() => window, window.GetType(), flags, groupNames);
        }

        /// <summary>
        /// Shows the specified <see cref="Window"/>
        /// </summary>
        /// <param name="createWindowFunc">The func for creating a new instance of the <see cref="Window"/></param>
        /// <param name="windowType">The type of <see cref="Window"/> to show</param>
        /// <param name="flags">The flags</param>
        /// <param name="groupNames">The group names for this instance</param>
        /// <returns>True if the <see cref="Window"/> is showing or false if it is not</returns>
        public static bool ShowWindow(Func<Window> createWindowFunc, Type windowType, ShowWindowFlags flags = ShowWindowFlags.None, params string[] groupNames)
        {
            // Lock to the current application
            lock (Application.Current)
            {
                try
                {
                    // Remove unused entries
                    Windows.RemoveWhere(x => !x.Key.TryGetTarget(out Window _));

                    RL.Logger?.LogDebugSource($"A custom window of type {windowType} has been requested to show");

                    // Get the currently available windows and copy them to a list
                    var windows = Application.Current.Windows.Cast<Window>().ToList();

                    // If no duplicates are allowed, make sure there is no Window of the same type
                    if (!flags.HasFlag(ShowWindowFlags.DuplicatesAllowed))
                    {
                        var window = windows.Find(x => windowType == x.GetType());

                        if (window != null)
                        {
                            RL.Logger?.LogInformationSource($"The window is not being shown due to a window of the same type being available");

                            if (!flags.HasFlag(ShowWindowFlags.DoNotFocusBlockingWindow))
                                window.Focus();

                            return false;
                        }
                    }

                    foreach (Window window in windows)
                    {
                        // Find saved instance
                        var gn = Windows.FindItem(k =>
                        {
                            k.Key.TryGetTarget(out Window win);
                            return win == window;
                        }).Value?.GroupNames;

                        // Continue if not found
                        if (gn?.FindItem(groupNames.Contains) == null)
                            continue;

                        RL.Logger?.LogInformationSource($"The window is not being shown due to a window with the same ID being available");

                        if (!flags.HasFlag(ShowWindowFlags.DoNotFocusBlockingWindow))
                            window.Focus();

                        return false;
                    }

                    // Create a new instance
                    var instance = createWindowFunc();

                    // Save the instance
                    Windows.Add(new WeakReference<Window>(instance), new WindowFlagModel(groupNames));

                    // Show the window
                    instance.Show();

                    RL.Logger?.LogInformationSource($"The window of type {windowType} has been shown");

                    return true;
                }
                catch (Exception ex)
                {
                    ex.HandleError("Custom showing window");
                    throw;
                }
            }
        }

        #endregion

        #region Public Enum

        /// <summary>
        /// The available flags to use on <see cref="AppWindowsManager.ShowWindow{Win}(WindAppWindowsManagerwWindowFlags,String[])"/>
        /// </summary>
        [Flags]
        public enum ShowWindowFlags
        {
            /// <summary>
            /// No flags
            /// </summary>
            None = 0,

            /// <summary>
            /// Indicates that several instances of the same <see cref="Window"/> are allowed
            /// </summary>
            DuplicatesAllowed = 1 << 0,

            /// <summary>
            /// Indicates if the blocking window preventing the current one to be shown should be focused
            /// </summary>
            DoNotFocusBlockingWindow = 1 << 1,
        }

        #endregion

        #region Private Class

        /// <summary>
        /// Model for window flags set using
        /// </summary>
        private class WindowFlagModel
        {
            #region Constructor

            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="groupNames">The group names for this instance</param>
            public WindowFlagModel(params string[] groupNames)
            {
                GroupNames = groupNames;
            }

            #endregion

            #region Public Properties

            /// <summary>
            /// The group names for this instance
            /// </summary>
            public string[] GroupNames { get; }

            #endregion
        }

        #endregion
    }
}