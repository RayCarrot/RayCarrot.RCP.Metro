using RayCarrot.CarrotFramework.Abstractions;

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
        public static AppViewModel App => RCF.GetService<AppViewModel>();
    }
}