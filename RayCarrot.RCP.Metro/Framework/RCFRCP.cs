using RayCarrot.CarrotFramework;
using RayCarrot.UserData;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Shortcuts for the Carrot Framework
    /// </summary>
    public static class RCFRCP
    {
        /// <summary>
        /// The application user data
        /// </summary>
        public static AppUserData Data => RCFData.UserDataCollection.GetUserData<AppUserData>();

        /// <summary>
        /// The app handler
        /// </summary>
        public static AppHandler App => RCF.GetService<AppHandler>();

        /// <summary>
        /// The file manager
        /// </summary>
        public static RCPFileManager File => RCF.GetService<RCPFileManager>();       

        /// <summary>
        /// The game manager
        /// </summary>
        public static GameManager Game => RCF.GetService<GameManager>();
    }
}