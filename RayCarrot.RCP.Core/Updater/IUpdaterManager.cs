using System.Threading.Tasks;

namespace RayCarrot.RCP.Core
{
    /// <summary>
    /// Defines a generic update manager
    /// </summary>
    public interface IUpdaterManager
    {
        /// <summary>
        /// Checks for updates
        /// </summary>
        /// <param name="forceUpdate">Indicates if the latest available version should be returned even if it's not newer than the current version</param>
        /// <param name="includeBeta">Indicates if beta updates should be included in the check</param>
        /// <returns>The result</returns>
        Task<UpdaterCheckResult> CheckAsync(bool forceUpdate, bool includeBeta);
    }
}