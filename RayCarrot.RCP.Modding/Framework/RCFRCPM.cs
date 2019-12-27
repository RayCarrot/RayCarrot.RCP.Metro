using RayCarrot.Extensions;
using RayCarrot.RCP.UI;
using RayCarrot.UserData;

namespace RayCarrot.RCP.Modding
{
    /// <summary>
    /// Shortcuts to the Carrot Framework for the Rayman Modding Panel
    /// </summary>
    public static class RCFRCPM
    {
        /// <summary>
        /// The app view model
        /// </summary>
        public static AppViewModel App => RCFRCPUI.App.CastTo<AppViewModel>();

        /// <summary>
        /// The application user data
        /// </summary>
        public static AppUserData Data => RCFData.UserDataCollection.GetUserData<AppUserData>();
    }
}