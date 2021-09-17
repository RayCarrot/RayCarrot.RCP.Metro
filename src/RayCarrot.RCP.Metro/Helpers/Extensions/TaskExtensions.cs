using System.Threading.Tasks;
using NLog;

namespace RayCarrot.RCP.Metro
{
    public static class TaskExtensions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void WithoutAwait(this Task task, string exceptionLogMessage)
        {
            // This will run the task without waiting for it to finish. If any exceptions are thrown they would normally be swallowed,
            // thus we log them instead
            task.ContinueWith(x => Logger.Error(x.Exception, exceptionLogMessage), TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}